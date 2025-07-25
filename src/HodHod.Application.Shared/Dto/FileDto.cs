using System;
using System.ComponentModel.DataAnnotations;

namespace HodHod.Dto;

public class FileDto
{
    public Guid? Id { get; set; }
    [Required]
    public string FileName { get; set; }

    public string FileType { get; set; }

    [Required]
    public string FileToken { get; set; }
    public string Base64Data { get; set; }

    public FileDto()
    {

    }

    public FileDto(string fileName, string fileType)
    {
        FileName = fileName;
        FileType = fileType;
        FileToken = Guid.NewGuid().ToString("N");
    }
}

