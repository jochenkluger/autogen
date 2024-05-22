using AutoGen.OpenAI;
using AutoGen;
using AutoGen.Core;
using AutoGen.DotnetInteractive;
using DotnetTeamSample;

// Ask user for work directory and create work directory if it does not exist
Console.WriteLine("Please enter work directory:");
var workDir = Console.ReadLine();
if (string.IsNullOrWhiteSpace(workDir))
{
    workDir = "C:\\tmp\\autogen";
}

if (Directory.Exists(workDir) is false)
{
    Directory.CreateDirectory(workDir);
}

using var service = new InteractiveService(workDir);
var dotnetInteractiveFunctions = new DotnetInteractiveFunction(service);

Console.WriteLine("Interactive service initialized");

await service.StartAsync(workDir, default);

Console.WriteLine("Interactive service started");

var gptConfig = LLMConfiguration.GetAzureOpenAIGPT3_5_Turbo();

var helperAgent = new GPTAgent(
    name: "helper",
    systemMessage: "You are a helpful AI assistant",
    temperature: 0f,
    config: gptConfig);

var groupAdmin = new GPTAgent(
    name: "groupAdmin",
    systemMessage: "You are the admin of the group chat",
    temperature: 0f,
    config: gptConfig);

var userProxy = new UserProxyAgent(name: "user", defaultReply: GroupChatExtension.TERMINATE, humanInputMode: HumanInputMode.NEVER)
    .RegisterPrintMessage();

// Create admin agent
var admin = AgentFactory.GetAdmin();

// create coder agent
// The coder agent is a composite agent that contains dotnet coder, code reviewer and nuget agent.
// The dotnet coder write dotnet code to resolve the task.
// The code reviewer review the code block from coder's reply.
// The nuget agent install nuget packages if there's any.
var coderAgent = AgentFactory.GetCoder();

// code reviewer agent will review if code block from coder's reply satisfy the following conditions:
// - There's only one code block
// - The code block is csharp code block
// - The code block is top level statement
// - The code block is not using declaration
var codeReviewAgent = AgentFactory.GetCodeReviewer();

// create runner agent
// The runner agent will run the code block from coder's reply.
// It runs dotnet code using dotnet interactive service hook.
// It also truncate the output if the output is too long.
var runner = AgentFactory.GetRunner(service, workDir);

var fileSystemFunctionsInstance = new FileSystemFunctions(workDir);
var fileSystemManager = AgentFactory.GetFileSystemManager(fileSystemFunctionsInstance);

Console.WriteLine("Agents initialized");

var adminToCoderTransition = Transition.Create(admin, coderAgent, async (from, to, messages) =>
{
    // the last message should be from admin
    var lastMessage = messages.Last();
    if (lastMessage.From != admin.Name)
    {
        return false;
    }

    return true;
});
var coderToReviewerTransition = Transition.Create(coderAgent, codeReviewAgent);
var adminToRunnerTransition = Transition.Create(admin, runner, async (from, to, messages) =>
{
    // the last message should be from admin
    var lastMessage = messages.Last();
    if (lastMessage.From != admin.Name)
    {
        return false;
    }

    if (lastMessage.GetContent()?.Contains("```file") is true)
    {
        return false;
    }

    // the previous messages should contain a message from coder
    var coderMessage = messages.FirstOrDefault(x => x.From == coderAgent.Name);
    if (coderMessage is null)
    {
        return false;
    }

    return true;
});

var adminToFileSystemManagerTransition = Transition.Create(admin, fileSystemManager, async (from, to, messages) =>
{
    // the last message should be from admin
    var lastMessage = messages.Last();
    if (lastMessage.From != admin.Name)
    {
        return false;
    }

    if (lastMessage.GetContent()?.Contains("```csharp") is true || lastMessage.GetContent()?.Contains("```powershell") is true)
    {
        return false;
    }

    // the previous messages should contain a message from coder
    var coderMessage = messages.FirstOrDefault(x => x.From == coderAgent.Name);
    if (coderMessage is null)
    {
        return false;
    }

    return true;
});

var runnerToAdminTransition = Transition.Create(runner, admin);

var fileSystemManagerToAdminTransition = Transition.Create(fileSystemManager, admin);

var reviewerToAdminTransition = Transition.Create(codeReviewAgent, admin);

var adminToUserTransition = Transition.Create(admin, userProxy, async (from, to, messages) =>
{
    // the last message should be from admin
    var lastMessage = messages.Last();
    if (lastMessage.From != admin.Name)
    {
        return false;
    }

    return true;
});

var userToAdminTransition = Transition.Create(userProxy, admin);

Console.WriteLine("Transitions initialized");

var workflow = new Graph(
    [
        adminToCoderTransition,
                coderToReviewerTransition,
                reviewerToAdminTransition,
                adminToFileSystemManagerTransition,
                adminToRunnerTransition,
                runnerToAdminTransition,
                adminToUserTransition,
                fileSystemManagerToAdminTransition,
                userToAdminTransition,
            ]);

Console.WriteLine("Workflow initialized");

// create group chat
var groupChat = new GroupChat(
    admin: groupAdmin,
    members: [admin, coderAgent, runner, fileSystemManager, codeReviewAgent, userProxy],
    workflow: workflow);

Console.WriteLine("Group chat initialized");
Console.WriteLine("".PadLeft(20, '='));
Console.WriteLine("Please enter Coding Task");
Console.WriteLine("".PadLeft(20, '-'));
var task = Console.ReadLine();
Console.WriteLine("".PadLeft(20, '='));

// task 1: retrieve the most recent pr from mlnet and save it in result.txt
var groupChatManager = new GroupChatManager(groupChat);
var conversationHistory = await userProxy.InitiateChatAsync(groupChatManager, task, maxRound: 30);
var lastMessage = conversationHistory.Last();

Console.WriteLine("".PadLeft(20, '='));
Console.WriteLine("Conversation Ended");
Console.WriteLine("".PadLeft(20, '-'));
Console.WriteLine(lastMessage.GetContent());
Console.WriteLine("".PadLeft(20, '='));

