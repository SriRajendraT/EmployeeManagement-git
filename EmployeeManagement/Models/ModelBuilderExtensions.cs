using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Models
{
    public static class ModelBuilderExtensions
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>().HasData(
                new Employee
                {
                    Id = 1,
                    Name = "Mark",
                    Email = "mark@gmail.com",
                    Department = Department.IT
                },
                new Employee
                {
                    Id = 2,
                    Name = "Mary",
                    Email = "mary@gmail.com",
                    Department = Department.Payroll
                }
             );
        }
    }
}
