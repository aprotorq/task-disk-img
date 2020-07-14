using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ReadSectors
{
    class Program
    {
        const int SECTORS_IN_BULK = 256;
        const int SECTOR_SIZE = 512;
        const int PATTERN_LENGTH = 8;

        static void Main(string[] args)
        {
            ReadBin();
        }
        static void ReadBin()
        {

            Sectors brokenSectors = new Sectors();
            byte[] pattern = { 0x57, 0x49, 0x50, 0x45, 0x52, 0x41, 0x50, 0x50 };

            using (var binaryReader = new BinaryReader(File.Open(@"E:\HitItGroup\disk.img", FileMode.Open)))
            {
                int seekPosition = 0;
                int fileLength = (int)binaryReader.BaseStream.Length;

                while (seekPosition < fileLength)
                {
                    //przewijamy stream i ściągamy bulk danych
                    binaryReader.BaseStream.Seek(seekPosition, SeekOrigin.Begin);
                    byte[] bulkBytes = binaryReader.ReadBytes(SECTORS_IN_BULK * SECTOR_SIZE);

                    //przechodzimy teraz po każdym sektorze
                    for (int sectorIndex = 0; sectorIndex < bulkBytes.Length / SECTOR_SIZE; sectorIndex++)
                    {
                        //wyciągamy jeden sektor z bulka
                        byte[] sectorBytes = new byte[SECTOR_SIZE];
                        Buffer.BlockCopy(bulkBytes, sectorIndex * SECTOR_SIZE, sectorBytes, 0, SECTOR_SIZE);

                        //przechodzimy przez 8 bajtowe fragmenty sektora
                        for (int sectorPartIndex = 0; sectorPartIndex < SECTOR_SIZE / PATTERN_LENGTH; sectorPartIndex++)
                        {
                            //wyciagamy kolejne 8 bajtowe fragmenty sektore
                            byte[] partBytes = new byte[PATTERN_LENGTH];
                            Buffer.BlockCopy(sectorBytes, sectorPartIndex * PATTERN_LENGTH, partBytes, 0, PATTERN_LENGTH);

                            //jeśli pattern jest taki sam jak fragment to szukamy dalej zaburzenia
                            if (partBytes.SequenceEqual(pattern))
                            {
                                continue;
                            }

                            //tutaj jest zaburzenie, podręcamy licznik i zapisujemy index sektora
                            brokenSectors.Count++;
                            brokenSectors.Positions.Add(seekPosition / SECTOR_SIZE + sectorIndex);
                            break;
                        }
                    }

                    seekPosition += SECTORS_IN_BULK * SECTOR_SIZE;
                }
            }

            string jsonString = JsonSerializer.Serialize(brokenSectors);

            string path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\export.txt";
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            else
            {
                File.WriteAllText(path, jsonString);
                Console.WriteLine("Data saved ");
            }
            Console.ReadKey();
        }
    }
}
