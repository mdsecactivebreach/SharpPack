using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.IO.Compression;
using Ionic.Zip;
class Archiver
{


    public byte[] DecryptArchiveAndGetFile(string zipfile, string retrievefile, string password)
    {
        byte[] unpacked = null;
        MemoryStream unzippedEntryStream = new MemoryStream();
        ZipFile zip = ZipFile.Read(zipfile);
        ZipEntry e = zip[retrievefile];
        e.ExtractWithPassword(unzippedEntryStream, password);
        unpacked =unzippedEntryStream.ToArray();
        return unpacked;
    }

    /*  Legacy methods replaced by DotNetZip
     *  
     *      public byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
     
        public byte[] Decrypt(string inFileName, string password)
        {
            byte[] saltValueBytes = Encoding.ASCII.GetBytes("SharpMacro");
            Rfc2898DeriveBytes passwordKey = new Rfc2898DeriveBytes(password, saltValueBytes);

            RijndaelManaged alg = new RijndaelManaged();
            alg.Key = passwordKey.GetBytes(alg.KeySize / 8);
            alg.IV = passwordKey.GetBytes(alg.BlockSize / 8);

            ICryptoTransform decryptor = alg.CreateDecryptor();
            FileStream inFile = new FileStream(inFileName, FileMode.Open, FileAccess.Read);
            CryptoStream decryptStream = new CryptoStream(inFile, decryptor, CryptoStreamMode.Read);
            byte[] fileData = new byte[inFile.Length];
            decryptStream.Read(fileData, 0, (int)inFile.Length);

            decryptStream.Close();
            inFile.Close();

            return fileData;
        }

        public byte[] ReadFileFromZipFile(byte[] zip, string zipfile)
        {
            byte[] unpacked = null;
            try
            {
                Stream data = new MemoryStream(zip);
                Stream unzippedEntryStream;
                ZipArchive archive = new ZipArchive(data);

                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.FullName == zipfile)
                    {
                        unzippedEntryStream = entry.Open();
                        unpacked = ReadFully(unzippedEntryStream);
                    }

                }

            }
            catch (Exception ex)
            {
                while (ex != null)
                {
                    Console.WriteLine(ex.Message);
                    ex = ex.InnerException;
                }
            }
            return unpacked;
        }*/

    public byte[] ArchiveHelper(string encpath, string passwd, string getfile)
    {
        //byte[] decrypted = Decrypt(encpath, passwd);
        //byte[] unpacked = ReadFileFromZipFile(decrypted, getfile);
        byte[] unpacked = DecryptArchiveAndGetFile(encpath, getfile, passwd);

        return unpacked;
    }

}
