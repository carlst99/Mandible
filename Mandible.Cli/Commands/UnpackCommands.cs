﻿using CommandDotNet;
using Mandible.Cli.Util;
using Mandible.Pack2;
using Mandible.Pack2.Names;
using Mandible.Services;
using Spectre.Console;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Mandible.Cli.Commands;

[Command(
    "unpack",
    Description = "Unpacks pack/pack2 file/s"
)]
public class UnpackCommands
{
    private readonly IAnsiConsole _console;
    private readonly CancellationToken _ct;

    public UnpackCommands(IAnsiConsole console, CancellationToken ct)
    {
        _console = console;
        _ct = ct;
    }

    [DefaultCommand]
    public async Task ExecuteAsync
    (
        [Operand(Description = "A path to a single pack/pack2 file, or a directory containing multiple.")]
        string inputPath,

        [Operand(Description = "The directory to output the packed content to. Contents will be nested in directories matching the name of the pack they originated from.")]
        string outputDirectory,

        [Operand(Description = "A path to a namelist file.")]
        string namelistPath
    )
    {
        bool packsDiscovered = CommandUtils.TryFindPacksFromPath
        (
            _console,
            inputPath,
            out List<string> packFiles,
            out List<string> pack2Files
        );

        if (!packsDiscovered)
            return;

        if (!Directory.Exists(outputDirectory))
        {
            if (!_console.Confirm("The output directory does not exist. Would you like to create it?"))
                return;

            Directory.CreateDirectory(outputDirectory);
        }

        Namelist namelist = await CommandUtils.BuildNamelistAsync(_console, namelistPath, _ct).ConfigureAwait(false);
        await ExportPack2AssetsAsync(pack2Files, outputDirectory, namelist).ConfigureAwait(false);
    }

    private async Task ExportPack2AssetsAsync
    (
        IReadOnlyList<string> pack2Files,
        string outputPath,
        Namelist namelist
    )
        => await _console.Progress()
            .StartAsync
            (
                async ctx =>
                {
                    ProgressTask exportTask = ctx.AddTask("Exporting pack2 assets...");
                    double increment = exportTask.MaxValue / pack2Files.Count;

                    foreach (string file in pack2Files)
                    {
                        using RandomAccessDataReaderService dataReader = new(file);
                        using Pack2Reader reader = new(dataReader);
                        string myOutputPath = Path.Combine(outputPath, Path.GetFileNameWithoutExtension(file));

                        if (!Directory.Exists(myOutputPath))
                            Directory.CreateDirectory(myOutputPath);

                        await reader.ExportAllAsync(myOutputPath, namelist, _ct).ConfigureAwait(false);

                        exportTask.Increment(increment);
                    }

                    _console.Markup("[green]Pack2 Export Complete![/]");
                }
            )
            .ConfigureAwait(false);
}