using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using AutoGen.Core;
using AutoGen.OpenAI.Extension;

namespace DotnetTeamSample
{
    /// <summary>
    /// Class FileSystemFunctions.
    /// This class contains the functions to interact with the file system.
    /// The class has to be public partial and the functions have to be public, so that AutoGen CodeGenerator can use them
    /// https://microsoft.github.io/autogen-for-net/articles/Create-type-safe-function-call.html
    /// </summary>
    public partial class FileSystemFunctions(string workdir)
    {

        //Function to list directory contents
        [Function]
        public async Task<string> ListDirectoryContents(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(workdir))
            {
                throw new ArgumentNullException(nameof(workdir));
            }

            var internalPath = Path.Combine(workdir, path);

            if (Directory.Exists(internalPath) == false)
            {
                Directory.CreateDirectory(internalPath);
            }

            var files = Directory.GetFiles(internalPath);
            var directories = Directory.GetDirectories(internalPath);
            var result = new StringBuilder();
            result.AppendLine("Files:");
            foreach (var file in files)
            {
                result.AppendLine(file);
            }

            result.AppendLine("Directories:");
            foreach (var directory in directories)
            {
                result.AppendLine(directory);
            }

            return result.ToString();
        }

        private class ListDirectoryContentsSchema
        {
            [JsonPropertyName(@"path")]
            public string? Path { get; set; }
        }

        public Task<string> ListDirectoryContentsWrapper(string arguments)
        {
            var schema = JsonSerializer.Deserialize<ListDirectoryContentsSchema>(
                arguments,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                });

            if (schema == null)
                throw new ArgumentNullException(nameof(schema));
            if (schema.Path == null)
                throw new ArgumentNullException(nameof(schema.Path));

            return ListDirectoryContents(schema.Path);
        }

        public FunctionContract ListDirectoryContentsFunctionContract
        {
            get => new FunctionContract
            {
                ClassName = @"FileSystemFunctions",
                Name = @"ListDirectoryContents",
                Description = @"List the files and directories in the given path inside the working directory",
                ReturnType = typeof(Task<string>),
                Parameters = new[]
                {
                    new FunctionParameterContract
                    {
                        Name = @"path",
                        Description = @"path to the directory inside the working directory that should be listed",
                        ParameterType = typeof(string),
                        IsRequired = true,
                    },
                },
            };
        }

        public global::Azure.AI.OpenAI.FunctionDefinition ListDirectoryContentsFunction
        {
            get => this.ListDirectoryContentsFunctionContract.ToOpenAIFunctionDefinition();
        }



        //Function to list directory contents recursively
        [Function]
        public async Task<string> ListDirectoryContentsRecursively(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(workdir))
            {
                throw new ArgumentNullException(nameof(workdir));
            }

            var internalPath = Path.Combine(workdir, path);

            if (Directory.Exists(internalPath) == false)
            {
                Directory.CreateDirectory(internalPath);
            }

            var files = Directory.GetFiles(internalPath, "*", SearchOption.AllDirectories);
            var directories = Directory.GetDirectories(internalPath, "*", SearchOption.AllDirectories);
            var result = new StringBuilder();
            result.AppendLine("Files:");
            foreach (var file in files)
            {
                result.AppendLine(file);
            }

            result.AppendLine("Directories:");
            foreach (var directory in directories)
            {
                result.AppendLine(directory);
            }

            return result.ToString();
        }

        private class ListDirectoryContentsRecursivelySchema
        {
            [JsonPropertyName(@"path")]
            public string? Path { get; set; }
        }

        public Task<string> ListDirectoryContentsRecursivelyWrapper(string arguments)
        {
            var schema = JsonSerializer.Deserialize<ListDirectoryContentsRecursivelySchema>(
                               arguments,
                                              new JsonSerializerOptions
                                              {
                                                  PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                                              });

            if (schema == null)
                throw new ArgumentNullException(nameof(schema));
            if (schema.Path == null)
                throw new ArgumentNullException(nameof(schema.Path));

            return ListDirectoryContentsRecursively(schema.Path);
        }

        public FunctionContract ListDirectoryContentsRecursivelyFunctionContract
        {
            get => new FunctionContract
            {
                ClassName = @"FileSystemFunctions",
                Name = @"ListDirectoryContentsRecursively",
                Description = @"List the files and directories in the given path inside the working directory recursively",
                ReturnType = typeof(Task<string>),
                Parameters = new[]
                {
                    new FunctionParameterContract
                    {
                        Name = @"path",
                        Description = @"path to the directory inside the working directory that should be listed",
                        ParameterType = typeof(string),
                        IsRequired = true,
                    },
                },
            };
        }

        public global::Azure.AI.OpenAI.FunctionDefinition ListDirectoryContentsRecursivelyFunction
        {
            get => this.ListDirectoryContentsRecursivelyFunctionContract.ToOpenAIFunctionDefinition();
        }


        //Function to Write content to a file
        [Function]
        public async Task<string> WriteToFile(string path, string content)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(workdir))
            {
                throw new ArgumentNullException(nameof(workdir));
            }

            var internalPath = Path.Combine(workdir, path);

            if (string.IsNullOrEmpty(content))
            {
                throw new ArgumentNullException(nameof(content));
            }

            File.WriteAllText(internalPath, content);
            return "File written successfully";
        }

        private class WriteToFileSchema
        {
            [JsonPropertyName(@"path")]
            public string? Path { get; set; }

            [JsonPropertyName(@"content")]
            public string? Content { get; set; }
        }

        public Task<string> WriteToFileWrapper(string arguments)
        {
            var schema = JsonSerializer.Deserialize<WriteToFileSchema>(
                               arguments,
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    });

            if (schema == null)
                throw new ArgumentNullException(nameof(schema));
            if (schema.Path == null)
                throw new ArgumentNullException(nameof(schema.Path));
            if (schema.Content == null)
                throw new ArgumentNullException(nameof(schema.Content));

            return WriteToFile(schema.Path, schema.Content);
        }

        public FunctionContract WriteToFileFunctionContract
        {
            get => new FunctionContract
            {
                ClassName = @"FileSystemFunctions",
                Name = @"WriteToFile",
                Description = @"Write content to a file inside the working directory",
                ReturnType = typeof(Task<string>),
                Parameters = new[]
                {
                    new FunctionParameterContract
                    {
                        Name = @"path",
                        Description = @"path to the file inside the working directory",
                        ParameterType = typeof(string),
                        IsRequired = true,
                    },
                    new FunctionParameterContract
                    {
                        Name = @"content",
                        Description = @"content to write to the file",
                        ParameterType = typeof(string),
                        IsRequired = true,
                    },
                },
            };
        }

        public global::Azure.AI.OpenAI.FunctionDefinition WriteToFileFunction
        {
            get => this.WriteToFileFunctionContract.ToOpenAIFunctionDefinition();
        }


        //Function to Get content of a file
        [Function]
        public async Task<string> GetFileContent(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(workdir))
            {
                throw new ArgumentNullException(nameof(workdir));
            }

            var internalPath = Path.Combine(workdir, path);

            if (File.Exists(internalPath) == false)
            {
                return "File not found";
            }

            return File.ReadAllText(internalPath);
        }

        private class GetFileContentSchema
        {
            [JsonPropertyName(@"path")]
            public string? Path { get; set; }
        }

        public Task<string> GetFileContentWrapper(string arguments)
        {
            var schema = JsonSerializer.Deserialize<GetFileContentSchema>(
                                              arguments,
                            new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                            });

            if (schema == null)
                throw new ArgumentNullException(nameof(schema));
            if (schema.Path == null)
                throw new ArgumentNullException(nameof(schema.Path));

            return GetFileContent(schema.Path);
        }

        public FunctionContract GetFileContentFunctionContract
        {
            get => new FunctionContract
            {
                ClassName = @"FileSystemFunctions",
                Name = @"GetFileContent",
                Description = @"Get the content of a file inside the working directory",
                ReturnType = typeof(Task<string>),
                Parameters = new[]
                {
                    new FunctionParameterContract
                    {
                        Name = @"path",
                        Description = @"path to the file inside the working directory",
                        ParameterType = typeof(string),
                        IsRequired = true,
                    },
                },
            };
        }

        public global::Azure.AI.OpenAI.FunctionDefinition GetFileContentFunction
        {
            get => this.GetFileContentFunctionContract.ToOpenAIFunctionDefinition();
        }


        //Function to delete file or directory
        [Function]
        public async Task<string> DeleteFileOrDirectory(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(workdir))
            {
                throw new ArgumentNullException(nameof(workdir));
            }

            var internalPath = Path.Combine(workdir, path);

            if (File.Exists(internalPath))
            {
                File.Delete(internalPath);
                return "File deleted successfully";
            }

            if (Directory.Exists(internalPath))
            {
                Directory.Delete(internalPath, true);
                return "Directory deleted successfully";
            }

            return "File or directory not found";
        }

        private class DeleteFileOrDirectorySchema
        {
            [JsonPropertyName(@"path")]
            public string? Path { get; set; }
        }

        public Task<string> DeleteFileOrDirectoryWrapper(string arguments)
        {
            var schema = JsonSerializer.Deserialize<DeleteFileOrDirectorySchema>(
                                                             arguments,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                });

            if (schema == null)
                throw new ArgumentNullException(nameof(schema));
            if (schema.Path == null)
                throw new ArgumentNullException(nameof(schema.Path));

            return DeleteFileOrDirectory(schema.Path);
        }

        public FunctionContract DeleteFileOrDirectoryFunctionContract
        {
            get => new FunctionContract
            {
                ClassName = @"FileSystemFunctions",
                Name = @"DeleteFileOrDirectory",
                Description = @"Delete a file or directory inside the working directory",
                ReturnType = typeof(Task<string>),
                Parameters = new[]
                {
                    new FunctionParameterContract
                    {
                        Name = @"path",
                        Description = @"path to the file or directory inside the working directory",
                        ParameterType = typeof(string),
                        IsRequired = true,
                    },
                },
            };
        }

        public global::Azure.AI.OpenAI.FunctionDefinition DeleteFileOrDirectoryFunction
        {
            get => this.DeleteFileOrDirectoryFunctionContract.ToOpenAIFunctionDefinition();
        }


        //Function to create directory
        [Function]
        public async Task<string> CreateDirectory(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(workdir))
            {
                throw new ArgumentNullException(nameof(workdir));
            }

            var internalPath = Path.Combine(workdir, path);

            if (Directory.Exists(internalPath) == false)
            {
                Directory.CreateDirectory(internalPath);
            }

            return "Directory created successfully";
        }


        private class CreateDirectorySchema
        {
            [JsonPropertyName(@"path")]
            public string? Path { get; set; }
        }

        public Task<string> CreateDirectoryWrapper(string arguments)
        {
            var schema = JsonSerializer.Deserialize<CreateDirectorySchema>(
                                                                            arguments,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                });

            if (schema == null)
                throw new ArgumentNullException(nameof(schema));
            if (schema.Path == null)
                throw new ArgumentNullException(nameof(schema.Path));

            return CreateDirectory(schema.Path);
        }

        public FunctionContract CreateDirectoryFunctionContract
        {
            get => new FunctionContract
            {
                ClassName = @"FileSystemFunctions",
                Name = @"CreateDirectory",
                Description = @"Create a directory inside the working directory",
                ReturnType = typeof(Task<string>),
                Parameters = new[]
                {
                    new FunctionParameterContract
                    {
                        Name = @"path",
                        Description = @"path to the directory inside the working directory",
                        ParameterType = typeof(string),
                        IsRequired = true,
                    },
                },
            };
        }

        public global::Azure.AI.OpenAI.FunctionDefinition CreateDirectoryFunction
        {
            get => this.CreateDirectoryFunctionContract.ToOpenAIFunctionDefinition();
        }
    }
}
