using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using System.Text.Json;

Console.WriteLine("Requiem Glam Patcher");
Console.WriteLine($"Mutagen.Bethesda version: {typeof(ISkyrimModGetter).Assembly.GetName().Version}");
Console.WriteLine($"Current working directory: {Directory.GetCurrentDirectory()}");

// Get data directory from command line arguments or use current directory
string dataDirectory = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
Console.WriteLine($"Using data directory: {dataDirectory}");

try
{
    // Step 1: Scan the load order for armor mods
    Console.WriteLine("\n=== Scanning Load Order ===");
    
    // Get all ESP/ESM files in the data directory
    var modFiles = Directory.GetFiles(dataDirectory, "*.esp")
        .Concat(Directory.GetFiles(dataDirectory, "*.esm"))
        .OrderBy(f => f)
        .ToList();
    
    var armorMods = new List<(string ModName, ISkyrimModGetter Mod)>();

    foreach (var modPath in modFiles)
    {
        try
        {
            var fileName = Path.GetFileName(modPath);
            var modKey = new ModKey(fileName, ModType.Plugin);
            var mod = SkyrimMod.CreateFromBinary(modPath, SkyrimRelease.SkyrimSE);
            
            if (modKey.Name.Equals("Skyrim.esm", StringComparison.OrdinalIgnoreCase) ||
                modKey.Name.Equals("Update.esm", StringComparison.OrdinalIgnoreCase) ||
                modKey.Name.Equals("Dawnguard.esm", StringComparison.OrdinalIgnoreCase) ||
                modKey.Name.Equals("HearthFires.esm", StringComparison.OrdinalIgnoreCase) ||
                modKey.Name.Equals("Dragonborn.esm", StringComparison.OrdinalIgnoreCase))
                continue;

            // Check if mod has Requiem as a master
            bool hasRequiemMaster = mod.ModHeader.MasterReferences.Any(master => 
                master.Master.FileName.String.Contains("Requiem", StringComparison.OrdinalIgnoreCase));

            if (hasRequiemMaster)
            {
                Console.WriteLine($"Skipping {modKey.Name} (Requiem patch)");
                continue;
            }

            // Check if mod contains armor records
            bool hasArmor = mod.Armors.Any();
            if (hasArmor)
            {
                armorMods.Add((modKey.Name, mod));
                Console.WriteLine($"Found armor mod: {modKey.Name} ({mod.Armors.Count()} armor records)");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading {Path.GetFileName(modPath)}: {ex.Message}");
        }
    }

    // Step 2: Print available mods
    Console.WriteLine("\n=== Available Armor Mods ===");
    if (!armorMods.Any())
    {
        Console.WriteLine("No armor mods found!");
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
        return;
    }

    for (int i = 0; i < armorMods.Count; i++)
    {
        Console.WriteLine($"{i + 1}. {armorMods[i].ModName}");
    }

    // Step 3: Let user select a mod
    Console.Write("\nSelect a mod to patch (enter number): ");
    string? input = Console.ReadLine();
    if (!int.TryParse(input, out int selection) || selection < 1 || selection > armorMods.Count)
    {
        Console.WriteLine("Invalid selection!");
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
        return;
    }

    var selectedMod = armorMods[selection - 1];
    Console.WriteLine($"Selected: {selectedMod.ModName}");

    // Step 4: Show armor sets in the selected mod
    Console.WriteLine("\n=== Armor Sets in Selected Mod ===");
    var armorGroups = selectedMod.Mod.Armors
        .GroupBy(a => GetArmorSlot(a))
        .Where(g => g.Any())
        .ToList();

    for (int i = 0; i < armorGroups.Count; i++)
    {
        var group = armorGroups[i];
        Console.WriteLine($"{i + 1}. {group.Key} ({group.Count()} items)");
        foreach (var armor in group.Take(3)) // Show first 3 items
        {
            Console.WriteLine($"   - {armor.EditorID}");
        }
        if (group.Count() > 3)
            Console.WriteLine($"   ... and {group.Count() - 3} more");
    }

    // Step 5: Let user select armor slot
    Console.Write("\nSelect armor slot to patch (enter number): ");
    input = Console.ReadLine();
    if (!int.TryParse(input, out int slotSelection) || slotSelection < 1 || slotSelection > armorGroups.Count)
    {
        Console.WriteLine("Invalid selection!");
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
        return;
    }

    var selectedSlot = armorGroups[slotSelection - 1];
    Console.WriteLine($"Selected slot: {selectedSlot.Key}");

    // Step 6: Show Requiem armor options for this slot
    Console.WriteLine("\n=== Requiem Armor Options ===");
    var requiemArmors = GetRequiemArmorsForSlot(selectedSlot.Key);
    for (int i = 0; i < requiemArmors.Count; i++)
    {
        Console.WriteLine($"{i + 1}. {requiemArmors[i]}");
    }

    // Step 7: Let user select Requiem armor type
    Console.Write("\nSelect Requiem armor type (enter number): ");
    input = Console.ReadLine();
    if (!int.TryParse(input, out int requiemSelection) || requiemSelection < 1 || requiemSelection > requiemArmors.Count)
    {
        Console.WriteLine("Invalid selection!");
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
        return;
    }

    var selectedRequiemArmor = requiemArmors[requiemSelection - 1];
    Console.WriteLine($"Selected Requiem armor: {selectedRequiemArmor}");

    // Step 8: Create the patch mod
    Console.WriteLine("\n=== Creating Patch ===");
    var patchMod = new SkyrimMod(new ModKey("RequiemGlamPatcher", ModType.Plugin), SkyrimRelease.SkyrimSE);
    
    // TODO: Implement the actual patching logic here
    // For now, just create a placeholder
    Console.WriteLine("Patch creation placeholder - TBD implementation");
    Console.WriteLine($"Would copy {selectedSlot.Count()} armor records from {selectedMod.ModName}");
    Console.WriteLine($"Would apply {selectedRequiemArmor} values to them");

    // Save the patch
    var outputPath = Path.Combine(dataDirectory, "RequiemGlamPatcher.esp");
    patchMod.WriteToBinary(outputPath);
    Console.WriteLine($"Saved patch to: {outputPath}");

    Console.WriteLine("\nPress any key to exit...");
    Console.ReadKey();
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine("\nPress any key to exit...");
    Console.ReadKey();
}

// Helper methods
static string GetArmorSlot(IArmorGetter armor)
{
    // This is a simplified slot detection - you might want to make this more sophisticated
    var name = armor.EditorID?.ToLower() ?? "";
    
    if (name.Contains("helmet") || name.Contains("hood") || name.Contains("hat"))
        return "Head";
    if (name.Contains("cuirass") || name.Contains("chest") || name.Contains("armor"))
        return "Body";
    if (name.Contains("gauntlet") || name.Contains("glove"))
        return "Hands";
    if (name.Contains("boot") || name.Contains("shoe"))
        return "Feet";
    if (name.Contains("shield"))
        return "Shield";
    
    return "Unknown";
}

static List<string> GetRequiemArmorsForSlot(string slot)
{
    // This is a placeholder - you'd want to load actual Requiem data
    return slot switch
    {
        "Head" => new List<string> { "Iron Helmet", "Steel Helmet", "Leather Helmet", "Elven Helmet", "Glass Helmet" },
        "Body" => new List<string> { "Iron Armor", "Steel Armor", "Leather Armor", "Elven Armor", "Glass Armor" },
        "Hands" => new List<string> { "Iron Gauntlets", "Steel Gauntlets", "Leather Gauntlets", "Elven Gauntlets", "Glass Gauntlets" },
        "Feet" => new List<string> { "Iron Boots", "Steel Boots", "Leather Boots", "Elven Boots", "Glass Boots" },
        "Shield" => new List<string> { "Iron Shield", "Steel Shield", "Leather Shield", "Elven Shield", "Glass Shield" },
        _ => new List<string> { "Unknown Armor Type" }
    };
}
