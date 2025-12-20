using System;
using System.IO;
using System.Text.Json;

namespace CheckInstallation
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== W40KRT AI Voiceover Mod - Installation Check ===\n");

            bool allChecksPassed = true;

            // Get expected installation path
            string localLowPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low";
            string expectedModPath = Path.Combine(localLowPath, "Owlcat Games", "Warhammer 40000 Rogue Trader", "UnityModManager", "W40KRT_AIVOMod");

            // Get actual executable location
            string actualPath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            // Check 1: Verify executable is in correct location
            Console.WriteLine("Check 1: Installation Location");

            if (string.Equals(actualPath, expectedModPath, StringComparison.OrdinalIgnoreCase))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓ PASS: Installation is in the correct location.\n");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("✗ FAIL: Executable is NOT in the correct location.");
                Console.WriteLine($"  Expected: {expectedModPath}");
                Console.WriteLine($"  Actual:   {actualPath}");
                Console.WriteLine("  Please ensure the mod is installed in the UnityModManager directory.\n");
                Console.ResetColor();
                allChecksPassed = false;

                // Skip remaining checks if not in correct location
                Console.WriteLine("Skipping remaining checks (executable not in correct location).\n");
                Console.WriteLine("===========================================");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Installation check failed.");
                Console.WriteLine("Please fix the issue above and run the check again.");
                Console.ResetColor();
                Console.WriteLine("===========================================\n");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            // Check 2: Verify voiceover pack is installed
            Console.WriteLine("Check 2: Voiceover Pack Installation");
            string oggFilePath = Path.Combine(actualPath, "tts", "1b", "1bfee5e6-ebfb-4b15-9803-1436722a5995.ogg");

            if (File.Exists(oggFilePath))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓ PASS: Voiceover pack is installed.\n");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("⚠ WARNING: Voiceover pack might not have been installed.");
                Console.WriteLine($"  Looking for: {oggFilePath}");
                Console.WriteLine("  Please ensure you have downloaded and installed the voiceover pack.\n");
                Console.ResetColor();
                allChecksPassed = false;
            }

            // Check 3: Look for conflicting SpeechMod installations
            Console.WriteLine("Check 3: Conflicting Mods");
            string unityModManagerPath = Path.Combine(localLowPath, "Owlcat Games", "Warhammer 40000 Rogue Trader", "UnityModManager");

            if (Directory.Exists(unityModManagerPath))
            {
                bool foundConflict = false;
                var modDirs = Directory.GetDirectories(unityModManagerPath);

                foreach (var modDir in modDirs)
                {
                    string modName = Path.GetFileName(modDir);

                    // Skip our own mod directory
                    if (string.Equals(modName, "W40KRT_AIVOMod", StringComparison.OrdinalIgnoreCase))
                        continue;

                    string infoJsonPath = Path.Combine(modDir, "Info.json");

                    if (File.Exists(infoJsonPath))
                    {
                        try
                        {
                            string jsonContent = File.ReadAllText(infoJsonPath);
                            using JsonDocument doc = JsonDocument.Parse(jsonContent);

                            if (doc.RootElement.TryGetProperty("Id", out JsonElement idElement))
                            {
                                string modId = idElement.GetString();

                                if (string.Equals(modId, "SpeechMod", StringComparison.OrdinalIgnoreCase))
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine($"✗ CONFLICT: Found old SpeechMod in: {modDir}");
                                    Console.WriteLine("            Please remove this old installation to avoid conflicts.");
                                    Console.ResetColor();
                                    foundConflict = true;
                                    allChecksPassed = false;
                                }
                            }
                        }
                        catch
                        {
                            // Silently skip mods with invalid Info.json
                        }
                    }
                }

                if (!foundConflict)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("✓ PASS: No conflicting SpeechMod installations found.\n");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine();
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"⚠ WARNING: UnityModManager directory not found at: {unityModManagerPath}\n");
                Console.ResetColor();
            }

            // Final summary
            Console.WriteLine("===================================================");
            if (allChecksPassed)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("All checks passed! Installation looks good.");
                Console.WriteLine("For configuration, use Ctrl+F10 inside of the game.");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Some checks failed or raised warnings.");
                Console.WriteLine("Please review the results above and fix any issues.");
                Console.ResetColor();
            }
            Console.WriteLine("===================================================\n");

            // Wait for input so window doesn't close immediately
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
