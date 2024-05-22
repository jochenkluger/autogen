using System.Diagnostics;
using System.Text;
using AutoGen.Core;

public static class PowerShellExecutionExtension
{
    /// <summary>
    /// Register an AutoReply hook to run dotnet code block from message.
    /// This hook will first detect if there's any Powershell code block (e.g. ```powershell and ```) in the most recent message.
    /// if there's any, it will run the code block and send the result back as reply.
    /// </summary>
    /// <param name="agent">agent</param>
    /// <param name="codeBlockPrefix">code block prefix</param>
    /// <param name="codeBlockSuffix">code block suffix</param>
    /// <param name="maximumOutputToKeep">maximum output to keep</param>
    public static IAgent RegisterPowerShellCodeBlockExectionHook(
        this IAgent agent,
        string workdir,
        string codeBlockPrefix = "```powershell",
        string codeBlockSuffix = "```",
        int maximumOutputToKeep = 500)
    {
        return agent.RegisterMiddleware(async (msgs, option, innerAgent, ct) =>
        {
            var lastMessage = msgs.LastOrDefault();
            if (lastMessage == null || lastMessage.GetContent() is null)
            {
                return await innerAgent.GenerateReplyAsync(msgs, option, ct);
            }

            // retrieve all code blocks from last message
            var codeBlocks =
                lastMessage.GetContent()!.Split(new[] { codeBlockPrefix }, StringSplitOptions.RemoveEmptyEntries);
            if (codeBlocks.Length <= 0)
            {
                return await innerAgent.GenerateReplyAsync(msgs, option, ct);
            }

            // run code blocks
            var result = new StringBuilder();
            var i = 0;
            result.AppendLine(@$"// [POWERSHELL_CODE_BLOCK_EXECUTION]");
            foreach (var codeBlock in codeBlocks)
            {
                var codeBlockIndex = codeBlock.IndexOf(codeBlockSuffix);

                if (codeBlockIndex == -1)
                {
                    continue;
                }

                // remove code block suffix
                var code = codeBlock.Substring(0, codeBlockIndex).Trim();

                if (code.Length == 0)
                {
                    continue;
                }

                var codeResult = RunPowerShellCommand(code, workdir);
                if (codeResult != null)
                {
                    result.AppendLine(@$"### Executing result for code block {i++}");
                    result.AppendLine(codeResult);
                    result.AppendLine("### End of executing result ###");
                }
            }

            if (result.Length <= maximumOutputToKeep)
            {
                maximumOutputToKeep = result.Length;
            }

            return new TextMessage(Role.Assistant, result.ToString().Substring(0, maximumOutputToKeep),
                from: agent.Name);
        });
    }

    public static string? RunPowerShellCommand(string command, string workdir)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = command,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = workdir
            }
        };

        process.Start();

        var sb = new StringBuilder();
        while (!process.StandardOutput.EndOfStream)
        {
            string? output = process.StandardOutput.ReadLine();
            sb.Append(output);
        }
        return sb.ToString();
    }

}
