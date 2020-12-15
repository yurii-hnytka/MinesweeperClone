using System.Windows;

namespace CourseProject {
    public partial class MainWindow : Window {
        public MainWindow() {
            this.InitializeComponent();

            this.DataContext = new MainWindowViewModel();   
        }
    }
}
