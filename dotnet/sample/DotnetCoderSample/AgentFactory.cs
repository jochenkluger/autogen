using AutoGen;
using AutoGen.Core;
using AutoGen.DotnetInteractive;
using AutoGen.OpenAI;

namespace DotnetTeamSample
{
    internal class AgentFactory
    {
        internal static AzureOpenAIConfig Config = LLMConfiguration.GetAzureOpenAIGPT4();

        internal static MiddlewareAgent<AssistantAgent> GetAdmin()
        {
            var agent = new AssistantAgent(
                    name: "admin",
                    systemMessage: """
                                   You are a manager who takes coding problem from user and resolve problem by splitting them into small tasks and assign each task to the most appropriate agent.
                                   Here's available agents who you can assign task to:
                                   - coder: write dotnet code to resolve task
                                   - runner: run dotnet code from coder

                                   The workflow is as follows:
                                   - You take the coding problem from user
                                   - You break the problem into small tasks. For each tasks you first ask coder to write code to resolve the task. Once the code is written, you ask runner to run the code.
                                   - Once a small task is resolved, you summarize the completed steps and create the next step.
                                   - You repeat the above steps until the coding problem is resolved.

                                   You can use the following json format to assign task to agents:
                                   ```task
                                   {
                                       "to": "{agent_name}",
                                       "task": "{a short description of the task}",
                                       "context": "{previous context from scratchpad}"
                                   }
                                   ```

                                   If you need to ask user for extra information, you can use the following format:
                                   ```ask
                                   {
                                       "question": "{question}"
                                   }
                                   ```

                                   Once the coding problem is resolved, summarize each steps and results and send the summary to the user using the following format:
                                   ```summary
                                   {
                                       "problem": "{coding problem}",
                                       "steps": [
                                           {
                                               "step": "{step}",
                                               "result": "{result}"
                                           }
                                       ]
                                   }
                                   ```

                                   Your reply must contain one of [task|ask|summary] to indicate the type of your message.
                                   """,
                    llmConfig: new ConversableAgentConfig
                    {
                        Temperature = 0,
                        ConfigList = [Config],
                    })
                .RegisterPrintMessage();
            Console.WriteLine($"Admin Agent initialized with Deployment: {Config.DeploymentName}");
            return agent;
        }

        internal static MiddlewareAgent<GPTAgent> GetCoder()
        {
            var agent = new GPTAgent(
                    name: "coder",
                    systemMessage: @"You act as dotnet coder, you write dotnet code to resolve task. Once you finish writing code, ask runner to run the code for you.

Here're some rules to follow on writing dotnet code:
- put code between ```csharp and ```
- When creating http client, use `var httpClient = new HttpClient()`. Don't use `using var httpClient = new HttpClient()` because it will cause error when running the code.
- Try to use `var` instead of explicit type.
- Try avoid using external library, use .NET Core library instead.
- Use top level statement to write code.
- Always print out the result to console. Don't write code that doesn't print out anything.

If you need to install nuget packages, put nuget packages in the following format:
```nuget
nuget_package_name
```

If your code is incorrect, Fix the error and send the code again.

Here's some externel information
- The link to mlnet repo is: https://github.com/dotnet/machinelearning. you don't need a token to use github pr api. Make sure to include a User-Agent header, otherwise github will reject it.
",
                    config: Config,
                    temperature: 0.4f)
                .RegisterPrintMessage();
            Console.WriteLine($"Coder Agent initialized with Deployment: {Config.DeploymentName}");
            return agent;
        }

        internal static MiddlewareAgent<GPTAgent> GetCodeReviewer()
        {
            var agent = new GPTAgent(
                    name: "reviewer",
                    systemMessage: """
                                   You are a code reviewer who reviews code from coder. You need to check if the code satisfy the following conditions:
                                   - The reply from coder contains at least one code block, e.g ```csharp and ```
                                   - There's only one code block and it's csharp code block
                                   - The code block is not inside a main function. a.k.a top level statement
                                   - The code block is not using declaration when creating http client

                                   You don't check the code style, only check if the code satisfy the above conditions.

                                   Put your comment between ```review and ```, if the code satisfies all conditions, put APPROVED in review.result field. Otherwise, put REJECTED along with comments. make sure your comment is clear and easy to understand.

                                   ## Example 1 ##
                                   ```review
                                   comment: The code satisfies all conditions.
                                   result: APPROVED
                                   ```

                                   ## Example 2 ##
                                   ```review
                                   comment: The code is inside main function. Please rewrite the code in top level statement.
                                   result: REJECTED
                                   ```

                                   """,
                    config: Config,
                    temperature: 0f)
                .RegisterPrintMessage();
            Console.WriteLine($"Code Reviewer Agent initialized with Deployment: {Config.DeploymentName}");
            return agent;
        }

        internal static MiddlewareAgent<IAgent> GetRunner(InteractiveService service)
        {
            var agent = new AssistantAgent(
                    name: "runner",
                    defaultReply: "No code available, coder, write code please")
                .RegisterDotnetCodeBlockExectionHook(interactiveService: service)
                .RegisterMiddleware(async (msgs, option, agent, ct) =>
                {
                    var mostRecentCoderMessage = msgs.LastOrDefault(x => x.From == "coder") ?? throw new Exception("No coder message found");
                    return await agent.GenerateReplyAsync(new[] { mostRecentCoderMessage }, option, ct);
                })
                .RegisterPrintMessage();
            Console.WriteLine("Runner Agent initialized (no GPT connection)");
            return agent;
        }

    }
}
