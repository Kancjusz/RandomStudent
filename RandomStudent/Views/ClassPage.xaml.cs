using RandomStudent.Models;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Xml.Linq;

namespace RandomStudent.Views;

public partial class ClassPage : ContentPage
{
    private int luckyNum = 0;

    public ClassPage()
    {
        InitializeComponent();
        InitializeLuckyNumber();
    }

    private void InitializeLuckyNumber()
    {

        string path = Path.Combine(FileSystem.AppDataDirectory, "classes.txt");
        XDocument xmlDocument = XDocument.Load(path);
        try
        {
            luckyNum = int.Parse(xmlDocument.Element("Classes").Attribute("LuckyNumber").Value);
        }
        catch (FormatException ex)
        {
            try
            {
                Random rnd = new Random();
                int max = xmlDocument.Element("Classes").Descendants("Class").Select(e => e.Element("Students").Elements().Count()).Max();
                luckyNum = rnd.Next(1, max);
                xmlDocument.Element("Classes").Attribute("Date").SetValue(DateTime.Today.ToString("d"));
                xmlDocument.Element("Classes").Attribute("LuckyNumber").SetValue(luckyNum);
                xmlDocument.Save(path);
            }
            catch (ArgumentOutOfRangeException ex2) { return; }
        }
        LuckyNumberLabel.Text = luckyNum.ToString();
    }

    private void SaveClass()
    {
        ClassModel model = (ClassModel)BindingContext;

        string path = Path.Combine(FileSystem.AppDataDirectory, "classes.txt");
        XDocument xmlDocument = XDocument.Load(path);

        xmlDocument.Element("Classes").Descendants("Class").Where(
            e => e.Element("Id").Value == model.Id.ToString()
        ).Single().Element("Students").ReplaceAll(model.Students.Select(s => new XElement(
                    "Student",
                    new XAttribute("PickedCountdown", s.PickedCountdown),
                    new XElement("Id", s.Id),
                    new XElement("Name", s.Name),
                    new XElement("Surname", s.Surname),
                    new XElement("IsPresent", s.IsPresent)
                ))
        );

        xmlDocument.Save(path);
    }

    private void Present_CheckedChange(object sender, CheckedChangedEventArgs e)
    {
        SaveClass();
    }

    private async void RandomStudentButton_Clicked(object sender, EventArgs e)
    {
        ClassModel model = (ClassModel)BindingContext;

        Random rnd = new Random();

        List<StudentModel> studentModels = model.Students.Where(s=>s.IsPresent && s.Id != luckyNum && s.PickedCountdown == 0).ToList();

        if(studentModels.Count == 0)
        {
            pickedNumber.Text = "";
            pickedStudent.Text = "";

            studentModels = model.Students.Where(s => s.IsPresent && s.Id != luckyNum).ToList();
            studentModels = studentModels.Where(s => s.PickedCountdown == studentModels.Select(s2 => s2.PickedCountdown).Min()).ToList();

            if (studentModels.Count == 0)
            {
                await DisplayAlert("B³¹d losowania", "Nie mo¿na wylosowaæ ucznia!", "Zamknij");
                return;
            }

            bool answer = await DisplayAlert("Wylosowaæ ucznia?",
                "W klasie obecne s¹ tylko osoby, które zosta³y ju¿\n niedawno wylosowane! (mniej ni¿ 3 kolejki temu)\n\nCzy mimo to chcesz wylosowaæ ucznia?",
                "Tak", "Nie");

            if (!answer) return;
        }       

        StudentModel student = studentModels[rnd.Next(0, studentModels.Count)];

        pickedNumber.Text = student.Id.ToString();
        pickedStudent.Text = student.Name + " " + student.Surname;

        UpdatePickedCountdown(student.Id-1);
    }

    private void UpdatePickedCountdown(int studentIndex)
    {
        ClassModel model = (ClassModel)BindingContext;

        foreach (StudentModel student in model.Students.Where(s => s.PickedCountdown > 0))
        {
            student.PickedCountdown--;
        };

        model.Students[studentIndex].PickedCountdown = 3;

        BindingContext = model;

        SaveClass();
    }

    private async void EditStudentListButton_Clicked(object sender, EventArgs e)
    {
        ClassModel model = (ClassModel)BindingContext;

        await Shell.Current.GoToAsync($"{nameof(EditClass)}?ClassId={model.Id}&Name={model.Name}&Students={JsonSerializer.Serialize(model.Students)}");
    }

    private async void BackButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(PickClass)}");
    }

    private void ShowStudentListButton_Clicked(object sender, EventArgs e)
    {
        ClassModel model = (ClassModel)BindingContext;
        if(!StudentListFrame.IsVisible && model.Students.Count == 0)
        {
            DisplayAlert("Wyœwietlanie listy uczniów", "List uczniów jest pusta!", "Zamknij");
            return;
        }

        StudentListFrame.IsVisible = !StudentListFrame.IsVisible;
        PickStudentFrame.WidthRequest = StudentListFrame.IsVisible ? 400 : 840;
    }
}