using RandomStudent.Models;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text.Json;
using System.Xml.Linq;

namespace RandomStudent.Views;

public partial class PickClass : ContentPage
{
    public PickClass()
    {
        InitializeComponent();
        LoadList();
    }

    private void LoadList()
    {
        string path = Path.Combine(FileSystem.AppDataDirectory, "classes.txt");

        XDocument xmlDocument;

        try
        {
            xmlDocument = XDocument.Load(path);

            List<ClassModel> classes = (
                from element in xmlDocument.Element("Classes").Elements()
                select new ClassModel()
                {
                    Id = int.Parse(element.Element("Id").Value),
                    Name = element.Element("Name").Value,
                    Students = new ObservableCollection<StudentModel>(
                        (from student in element.Element("Students").Elements()
                         orderby student.Element("Name").Value
                         select new StudentModel()
                         {
                             Id = int.Parse(student.Element("Id").Value),
                             Name = student.Element("Name").Value,
                             Surname = student.Element("Surname").Value,
                             IsPresent = bool.Parse(student.Element("IsPresent").Value),
                             PickedCountdown = int.Parse(student.Attribute("PickedCountdown").Value)
                         }).ToList()
                    )
                }
            ).ToList();

            if(xmlDocument.Element("Classes").HasElements && xmlDocument.Element("Classes").Attribute("Date").Value != DateTime.Today.ToString("d"))
            {
                Random rnd = new Random();
                try
                {
                    int max = xmlDocument.Element("Classes").Descendants("Class").Select(e => e.Element("Students").Elements().Count()).Max();
                    int luckyNum = rnd.Next(1, max);
                    xmlDocument.Element("Classes").Attribute("Date").SetValue(DateTime.Today.ToString("d"));
                    xmlDocument.Element("Classes").Attribute("LuckyNumber").SetValue(luckyNum);
                    xmlDocument.Save(path);
                }
                catch{}
            }

            BindingContext = new ClassListModel()
            {
                ClassList = classes
            };

            LuckyNumberLabel.Text = xmlDocument.Element("Classes").Attribute("LuckyNumber").Value;
        }
        catch (FileNotFoundException ex)
        {
            xmlDocument = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("Classes",
                    new XAttribute("Date", ""),
                    new XAttribute("LuckyNumber", "Brak")
                )
            );
            xmlDocument.Save(path);

            ClassListModel model = new ClassListModel();
            BindingContext = model;
        }
    }

    private async void AddNewClass_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(AddClass)}?ClassId={0}&Name={""}&Students={JsonSerializer.Serialize(new ObservableCollection<StudentModel>())}");
    }

    private async void classCollection_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        int id = (e.CurrentSelection.FirstOrDefault() as ClassModel).Id;
        ClassModel model = ((ClassListModel)BindingContext).ClassList[id];

        string s = JsonSerializer.Serialize(model.Students);
        await Shell.Current.GoToAsync($"{nameof(ClassPage)}?ClassId={id}&Name={model.Name}&Students={s}");
    }

    private async Task<ClassModel> DisplayClass(int index)
    {
        return await Task.Run(async () =>
        {
            string path = Path.Combine(FileSystem.AppDataDirectory, "classes.xml");
            XDocument xmlDocument = XDocument.Load(path);

            XElement currentClass = xmlDocument.Element("Classes").Descendants("Class").Where(e => e.Element("Id").Value == index.ToString()).Single();

            ClassModel model = new ClassModel()
            {
                Id = index,
                Name = currentClass.Element("Name").Value,
                Students = new ObservableCollection<StudentModel>()
            };

            model.Students = await GetStudents(currentClass);

            return model;
        });
    }

    private async Task<ObservableCollection<StudentModel>> GetStudents(XElement currentClass)
    {
        return await Task.Run(async () =>
        {
            var tasks = new List<Task<StudentModel>>();
            foreach (XElement xStudent in currentClass.Element("Students").Elements())
            {
                tasks.Add(GetStudent(xStudent));
            }

            List<StudentModel> students = (await Task.WhenAll(tasks)).ToList();

            return new ObservableCollection<StudentModel>(students);
        });
    }

    private async Task<StudentModel> GetStudent(XElement xStudent)
    {
        return new StudentModel()
        {
            Id = int.Parse(xStudent.Element("Id").Value),
            Name = xStudent.Element("Name").Value,
            Surname = xStudent.Element("Surname").Value,
            IsPresent = bool.Parse(xStudent.Element("IsPresent").Value),
            PickedCountdown = int.Parse(xStudent.Attribute("PickedCountdown").Value)
        };
    }
}