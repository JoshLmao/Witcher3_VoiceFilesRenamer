# The Witcher 3 Voice Line Files Renamer

A basic console application that renames all of the extracted voice line files from the Witcher 3 to what is being said. Before using this, **you must [follow this guide on how to extract all of the voice files](https://github.com/Gizm000/Extracting-Voice-Over-Audio-from-Witcher-3)**. It comes with a database which contains all of the data needed to rename all of the files. You **WILL** need to convert the .xlsx database file into a .csv file so that the console application can read the file data

If you don't know how to convert it to a .csv, [here's the one I used](Witcher3_VoiceFilesRenamer/Witcher3_VoiceFilesRenamer/Witcher3VoiceLineDatabaseFile-140817.csv) however it might differ from the one from the guide

## How To Use

**1:** Follow all of the steps inside [Gizm000's guide to extract all of the voice lines](https://github.com/Gizm000/Extracting-Voice-Over-Audio-from-Witcher-3)

**2:** Download the latest release from the [release page](https://github.com/JoshLmao/Witcher3_VoiceFilesRenamer/releases) and extract it to a folder

**3:** Run the Witcher3_VoiceFilesRenamer.exe through the command prompt with these arguments:

**3.1:** The program requires 3 arguments in this order. 

* A path to the .csv Database file. 

* The location of the originally extracted files (If you followed the linked guide it will be the created "ogg" and "wav" folders) 

* The path to where you want the voice lines to be extracted to. Just the root folder, this program handles sorting into folders by characters names

**4:** Run & wait


## Notes/F.A.Q's

* You only need to run this once on either the "ogg" folder or the "wav" folder. They all contains the same voice files
