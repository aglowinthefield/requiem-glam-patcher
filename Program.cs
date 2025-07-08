using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;

Console.WriteLine("Hello, World!");
Console.WriteLine($"Mutagen.Bethesda version: {typeof(ISkyrimModGetter).Assembly.GetName().Version}");

// Example of how to create a new mod
var mod = new SkyrimMod(new ModKey("RequiemGlamPatcher", ModType.Plugin), SkyrimRelease.SkyrimSE);
Console.WriteLine($"Created mod: {mod.ModKey}");

// Print current working directory
Console.WriteLine($"Current working directory: {Directory.GetCurrentDirectory()}");

// Wait for user input before closing
Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();
