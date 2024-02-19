namespace RandomStudent.Controls;

public partial class ClassControl : ContentView
{
	public static readonly BindableProperty ClassNameProperty = BindableProperty.Create(nameof(ClassName), typeof(string), typeof(ClassControl), string.Empty);
	   
	public string ClassName
	{
		get => (string)GetValue(ClassNameProperty);
		set => SetValue(ClassNameProperty, value);
	}

	public ClassControl()
	{
		InitializeComponent();
	}
}