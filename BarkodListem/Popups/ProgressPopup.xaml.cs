using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
namespace BarkodListem.Popups
{
    public partial class ProgressPopup : Popup
    {
        public ObservableCollection<StepModel> Steps { get; set; } = new ObservableCollection<StepModel>();
        public ProgressPopup()
        {
            InitializeComponent();
            stepsCollectionView.BindingContext = this;
        }
        public void AddStep(string text)
        {
            Steps.Add(new StepModel { Text = text, Status = "" });
            UpdateProgress();
        }
        public void CompleteStep(string text)
        {
            var step = Steps.FirstOrDefault(s => s.Text == text);
            if (step != null)
            {
                step.Status = "✔️";
                stepsCollectionView.ItemsSource = null;
                stepsCollectionView.ItemsSource = Steps;
                UpdateProgress();
            }
        }
        private void UpdateProgress()
        {
            if (Steps.Count == 0)
                progressBar.Progress = 0;
            else
            {
                var completedCount = Steps.Count(s => s.Status == "✔️");
                progressBar.Progress = (double)completedCount / Steps.Count;
                if (completedCount == Steps.Count)
                {
                    progressBar.Progress = 1.0;
                    btnTamam.IsEnabled = true;
                }
            }
        }
        private void btnTamam_Clicked(object sender, EventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() => this.CloseAsync());
        }
    }
    public class StepModel
    {
        public string Text { get; set; }
        public string Status { get; set; }
    }
}
