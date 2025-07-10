namespace ETPLib.FileUtil;

public class FileHandler
{
    private FileStream m_FileStream;
    
    private StreamWriter m_StreamWriter;
    public string DirectoryPath { get; set; }
    public string FileName { get; set; }

	public string FileNameWithoutExtension { get; set; }

    public string Extension { get; set; }
    
    public string FilePath { get; set; }
    
    protected string Intent { get; set; }

    public FileHandler(string path)
    {
        DirectoryPath = Path.GetDirectoryName(path);
        FileName = Path.GetFileName(path);
		FileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
        Extension = Path.GetExtension(path);
        FilePath = path;

        if (!Directory.Exists(DirectoryPath))
        {
			if (string.IsNullOrEmpty(DirectoryPath))
			{
				DirectoryPath = "./";
			}
			else {
				Directory.CreateDirectory(DirectoryPath);
			}
        }

        if (File.Exists(path))
        {
            File.Delete(path);
        }
        
        m_FileStream = File.Create(path);
        m_StreamWriter = new StreamWriter(m_FileStream);
    }

    public void WriteLine(string line, bool intent = false)
    {
        m_StreamWriter.WriteLine(intent?Intent + line:line);
    }

    public void Close()
    {
        if (m_StreamWriter != null)
        {
            m_StreamWriter.Close();
        }

        if (m_FileStream != null)
        {
            m_FileStream.Close();
        }
    }

    protected void AddIntent(int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            Intent += "\t";
        }
    }

    protected void SubIntent(int count = 1)
    {
        if (Intent.Length <= 0) return;
        Intent = Intent.Substring(0, Intent.Length-count);
    }

    public virtual void GenerateContent()
    {
        
    }
    
}