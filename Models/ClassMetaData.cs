using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace SchoolManagementApp.MVC.Data;

public class ClassMetaData
{
    [Display(Name = "Lecturer")]
    public int LectureId { get; set; }

    [Display(Name = "Course")]
    public int CourseId { get; set; }
}

[ModelMetadataType(typeof(ClassMetaData))]
public partial class Class { }