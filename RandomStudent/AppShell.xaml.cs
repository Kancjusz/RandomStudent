using RandomStudent.Views;

namespace RandomStudent
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {

            InitializeComponent();

            Routing.RegisterRoute("PickClass", typeof(PickClass));
            Routing.RegisterRoute("AddClass", typeof(AddClass));
            Routing.RegisterRoute("ClassPage", typeof(ClassPage));
            Routing.RegisterRoute("EditClass", typeof(EditClass));

            
        }
    }
}
