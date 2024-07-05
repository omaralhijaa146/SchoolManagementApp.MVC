using SchoolManagementApp.MVC.Data;

namespace SchoolManagementApp.MVC.Models;

public class ClassEnrollmentVM
{

    public ClassViewModel? Class { get; set; }
    public List<StudentsEnrollmentVM> Students { get; set; } = new List<StudentsEnrollmentVM>();
}

