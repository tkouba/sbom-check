using SbomCheck.Cli;
using Spectre.Console.Cli;

var app = new CommandApp<CheckCommand>();
return app.Run(args);
