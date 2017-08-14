using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NameAllWitcherFiles
{
    public class VoiceLineData
    {
        /// <summary>
        /// The original file name hex code
        /// </summary>
        public string HexCode { get; set; }
        /// <summary>
        /// The voice line formatted as a file name
        /// </summary>
        public string VoiceLineFileName { get; set; }
        /// <summary>
        /// The character that says it. Put in folder
        /// </summary>
        public string Character { get; set; }
    }

    class Program
    {
        static readonly int LONGEST_FILE_PATH = 248;
        static List<VoiceLineData> CreatedData = new List<VoiceLineData>();

        /// <summary>
        /// Console application to rename all extracted audio from The Witcher 3 using this guide.
        /// Make sure to comeplete all these steps before running this app
        /// https://github.com/Gizm000/Extracting-Voice-Over-Audio-from-Witcher-3
        /// 
        /// Example Command Line Usage:
        /// Witcher3_VoiceFilesRenamer.exe "C:\Databasefile.csv" "C:\Witcher3_VoiceLines\" "C:\SortedVoiceLines\"
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            CreatedData = new List<VoiceLineData>();

            string databasePath = args[0];
            string originalFilesPath = args[1];
            string extractToRootFolder = args[2];

            bool argsError = !File.Exists(databasePath) || !Directory.Exists(originalFilesPath) || !Directory.Exists(extractToRootFolder);
            if(argsError)
            {
                Console.WriteLine("Can't start program. Invalid arguments. Database file should exists & all directories should already be created by the user");
                return;
            }

            if(Path.GetExtension(databasePath) != ".csv")
            {
                Console.WriteLine("Database should be in the .csv format. Load Excel and Save As .csv");
                return;
            }

            string[] databaseLines = File.ReadAllLines(databasePath);
            foreach (string line in databaseLines)
            {
                if (string.IsNullOrEmpty(line))
                    continue;

                //Quick verification that line is valid for use
                string[] split = line.Split(' ');
                if (split.Length < 2)
                    continue;

                VoiceLineData data = GetVoiceLineFromLine(line);
                if (data == null)
                    continue;

                string originalName = data.HexCode;
                string voiceLineName = data.VoiceLineFileName;
                string characterFolderName = data.Character;

                string extension = GetExtensionFromPathWithout(originalFilesPath, originalName);
                string voiceLinePath = $"{originalFilesPath}\\{originalName}{extension}";
                if (File.Exists(voiceLinePath))
                {
                    string characterDir = $"{extractToRootFolder}\\{characterFolderName}";
                    if (!Directory.Exists(characterDir))
                        Directory.CreateDirectory(characterDir);

                    string newFileName = $"{voiceLineName}{extension}";
                    string actualPath = Path.Combine(extractToRootFolder, characterDir, newFileName);

                    if (actualPath.Length > LONGEST_FILE_PATH)
                    {
                        actualPath = MakeFilePathLegal(voiceLineName, Path.Combine(extractToRootFolder, characterDir), extension);
                    }

                    //If file exists, just skip. Could cause problems if multiple voice lines with same text
                    if(File.Exists(actualPath))
                    {
                        continue;
                    }

                    try
                    {
                        File.Move(voiceLinePath, actualPath);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Created Voice Line file \"{voiceLineName}\"");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Error saving {actualPath}");
                        Console.ForegroundColor = ConsoleColor.White;
                    }

                    CreatedData.Add(data);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Can't find file {voiceLinePath}");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
        }


        /// <summary>
        /// Parses the line and gets the original file name (the hex code), the character that said it and the voice line as a file name
        /// </summary>
        /// <param name="line"></param>
        /// <returns>The hex code original name & the new file name as a voice line</returns>
        static VoiceLineData GetVoiceLineFromLine(string line)
        {
            VoiceLineData data = new VoiceLineData();
            string[] split = line.Split(' ');

            //The index of the hex code
            int hexCodeIndex = -1;
            //The final
            int colonIndex = -1;
            for (int i = 0; i < split.Length; i++)
            {
                if (string.IsNullOrEmpty(split[i]))
                    continue;

                string character = split[i];
                if (split[i].Contains(':') && colonIndex == -1)
                    colonIndex = i;
                else if (split[i].Contains("0x0") && hexCodeIndex == -1)
                    hexCodeIndex = i;
            }

            if (hexCodeIndex == -1 || colonIndex == -1)
                return null;

            //Get character folder
            string characterFolderName = "";
            for (int i = hexCodeIndex + 1; i < colonIndex + 1; i++)
            {
                if (string.IsNullOrEmpty(split[i]))
                    continue;

                //Remove colon
                string formatted = split[i];
                if (formatted.Contains(':'))
                    formatted = split[i].Replace(":", "");

                characterFolderName += formatted;
            }
            data.Character = characterFolderName;


            //Convert data to file name
            string voiceLineFileName = "";
            for (int i = colonIndex + 1; i < split.Length; i++)
            {
                if (string.IsNullOrEmpty(split[i]))
                    continue;

                voiceLineFileName += RemoveInvalidChars(split[i]);

                if (i < split.Length - 1)
                {
                    voiceLineFileName += "_";
                }
            }
            data.VoiceLineFileName = voiceLineFileName;

            data.HexCode = split[hexCodeIndex];

            return data;
        }

        static string RemoveInvalidChars(string invalidString)
        {
            string formatted = invalidString;
            var invalidChars = Path.GetInvalidFileNameChars().ToList();
            for (int i = 0; i < invalidChars.Count; i++)
            {
                if (formatted.Contains(invalidChars[i]))
                {
                    formatted = formatted.Replace(invalidChars[i].ToString(), "");
                }
            }

            return formatted;
        }

        /// <summary>
        /// Converts a file path to be within LONGEST_FILE_PATH chars
        /// </summary>
        /// <param name="fileName">Just the file name</param>
        /// <param name="newSaveLocation">the save location of the new file</param>
        /// <param name="extension">the extension needed to be on file path</param>
        /// <returns></returns>
        static string MakeFilePathLegal(string fileName, string newSaveLocation, string extension)
        {
            string allPath = Path.Combine(newSaveLocation, $"{fileName}{extension}");
            if (allPath.Length < LONGEST_FILE_PATH)
                return fileName;

            int lengthNeeded = newSaveLocation.Length + extension.Length;
            int charAmountLeft = LONGEST_FILE_PATH - lengthNeeded;
            string cutDownName = extension;

            int addCharsCount = LONGEST_FILE_PATH - lengthNeeded;
            //Track how many weve added and insert new ones there
            int addedCharsCount = 0;
            for (int i = 0; i < fileName.Length; i++)
            {
                if (cutDownName.Length < addCharsCount)
                {
                    if (cutDownName.Length > addCharsCount - 3)
                    {
                        //Add ellipse at end. Have . for extension included in our ellipse
                        cutDownName = cutDownName.Insert(addedCharsCount, ".");
                        addedCharsCount++;
                    }
                    else
                    {
                        cutDownName = cutDownName.Insert(addedCharsCount, fileName[i].ToString());
                        addedCharsCount++;
                    }
                }
                else
                    break;
            }


            return Path.Combine(newSaveLocation, cutDownName);
        }

        /// <summary>
        /// Gets all the extension from the converted voice file
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="fileNameWithoutExt"></param>
        /// <returns></returns>
        static string GetExtensionFromPathWithout(string folderPath, string fileNameWithoutExt)
        {
            string[] allFiles = Directory.GetFiles(folderPath);

            int firstFullStopIndex = -1;
            string first = allFiles.First();
            for (int i = 0; i < first.Length; i++)
            {
                if (first[i] == '.')
                {
                    firstFullStopIndex = i;
                    break;
                }
            }

            string fullExt = "";
            for (int i = firstFullStopIndex; i < first.Length; i++)
            {
                fullExt += first[i];
            }
            return fullExt;
        }
    }
}
