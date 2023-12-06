using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.Models
{
    public class MockEmployeeRepository : IEmployeeRepository
    {
        private List<Employee> _employeeList;

        public MockEmployeeRepository()
        {
            _employeeList = new List<Employee>()
            {
                new Employee(){Id=1,Name="Mary",Email="Mary@gmail.com",Department=Department.HR},
                new Employee(){Id=2,Name="Jhon",Email="Jhon@gmail.com",Department=Department.IT},
                new Employee(){Id=3,Name="Sam",Email="Sam@gmail.com",Department=Department.IT}
            };
        }

        public Employee AddEmployee(Employee employee)
        {
            employee.Id = _employeeList.Max(e => e.Id) + 1;
            _employeeList.Add(employee);
            return employee;
        }

        public Employee Delete(int id)
        {
            Employee employee=_employeeList.FirstOrDefault(e => e.Id == id);
            if (employee != null)
            {
                _employeeList.Remove(employee);
            }
            return employee;
        }

        public Employee GetEmployee(int id)
        {
            return _employeeList.FirstOrDefault(e => e.Id == id);
        }

        public IEnumerable<Employee> GetEmployees()
        {
            return _employeeList;
        }

        public Employee UpdateEmployee(Employee employee)
        {
           Employee employee1= _employeeList.FirstOrDefault(e => e.Id == employee.Id);
            if (employee1 != null)
            {
                employee1.Name = employee.Name;
                employee1.Email = employee.Email;
                employee1.Department = employee.Department;
            }
            return employee1;
        }
    }
}
