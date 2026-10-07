using Data_Hospital_Manager;
using Data_Hospital_Manager.Entities;
using Hospital_Manager.ViewModels.Patient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Hospital_Manager.Controllers
{
    public class PatientController : Controller
    {
        private readonly HospitalDbContext context;
        public PatientController(HospitalDbContext dbcontext)
        {
            this.context = dbcontext;
        }

        public async Task<IActionResult> Index()
        {
            var patinets = await context.Patients
                .Include(p => p.DoctorPatients)
                .ThenInclude(dp => dp.Doctor)
                .ToArrayAsync();

            var model = new List<PetientIndex>();
            foreach (var patient in patinets)
            {
                model.Add(new PetientIndex
                {
                    Id = patient.Id,
                    FirstName = patient.FirstName,
                    LastName = patient.LastName,
                    Email = patient.Email,
                    PhoneNumber = patient.PhoneNumber,

                    DoctorName = string.Join(",", patient.DoctorPatients.Select(dp => dp.Doctor.FirstName + " " + dp.Doctor.LastName))
                });
            }
            return View(model);
        }
        public async Task<IActionResult> Details(int id)
        {
            var patient = await context.Patients
                .Include(p => p.DoctorPatients)
                .ThenInclude(dp => dp.Doctor)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null)
            {
                return NotFound();


            }
            var model = new PatientDeteils
            {
                Id = patient.Id,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                Email = patient.Email,
                PhoneNumber = patient.PhoneNumber,

                DoctorsName = string.Join(", ", patient.DoctorPatients.Select(dp => dp.Doctor.FirstName + " " + dp.Doctor.LastName))
            };
            return View(model);
        }
        public async Task LoadDoctors(List<int> selectedDoctors = null)
        {
            var doctors = await context.Doctors
                .Select(d => new
                {
                    d.Id,
                    FullName = d.FirstName + " " + d.LastName,
                })
                .ToArrayAsync();
            ViewBag.Doctors = new MultiSelectList(
                doctors, "Id", "FullName", selectedDoctors

                );
        }
        public async Task<IActionResult> Create()
        {
            await LoadDoctors();
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(PatientCreate model)
        {
            if (ModelState.IsValid)
            {
                var patient = new Patient
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber

                };
                foreach (var item in model.Doctorids)
                {
                    patient.DoctorPatients.Add(new DoctorPatient
                    {
                        DoctorId = item
                    });
                }
                context.Patients.Add(patient);
                await context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            await LoadDoctors(model.Doctorids);
            return View(model);
        }
        public async Task<IActionResult> Edit(int id)
        {
            var patient = await context.Patients
                .Include(p => p.DoctorPatients)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (patient == null)
            {
                return NotFound();
            }
            var model = new PatinetEdit
            {
                Id = patient.Id,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                Email = patient.Email,
                PhoneNumber = patient.PhoneNumber,
                Doctorids = patient.DoctorPatients
                .Select(dp => dp.DoctorId).ToList()

            };
            await LoadDoctors(model.Doctorids);
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int id, PatinetEdit model)
        {

            if (id != model.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                var patient = await context.Patients.Include(p => p.DoctorPatients).FirstOrDefaultAsync(p => p.Id == id);
                if (patient == null)
                {
                    return NotFound();
                }

                patient.FirstName = model.FirstName;
                patient.LastName = model.LastName;
                patient.Email = model.Email;
                patient.PhoneNumber = model.PhoneNumber;

                var existingDoctorIds = patient.DoctorPatients.Select(dp => dp.DoctorId).ToList();
                var newDoctorIds = model.Doctorids ?? new List<int>();

                foreach (var doctorId in existingDoctorIds.Except(newDoctorIds))
                {
                    var doctorPatient = patient.DoctorPatients.FirstOrDefault(dp => dp.DoctorId == doctorId);
                    if (doctorPatient != null)
                    {
                        context.Remove(doctorPatient);
                    }
                }

                foreach (var doctorId in newDoctorIds.Except(existingDoctorIds))
                {
                    context.Add(new Data_Hospital_Manager.Entities.DoctorPatient { DoctorId = doctorId, PatientId = patient.Id });
                }

                await context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            await LoadDoctors(model.Doctorids);
            return View(model);
        }
        public async Task<IActionResult> Delete(int id)
        {
            var patient = await context.Patients.FindAsync(id);
            if (patient == null)
            {
                return NotFound();

            }
            var model = new PatientDelete
            {
                Id = patient.Id,
                FirstName = patient.FirstName,
                LastName = patient.LastName

            };
            return View(model);
        }
        [HttpPost]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var patient = await context.Patients.FindAsync(id);
            if (patient != null)
            {
                context.Patients.Remove(patient);
                await context.SaveChangesAsync();

            }
            return RedirectToAction(nameof(Index));
        }
    }
}
