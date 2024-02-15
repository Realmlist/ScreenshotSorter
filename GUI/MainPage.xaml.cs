using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;

namespace ScreenshotSorter;

public partial class MainPage : ContentPage
{
    public double moveLayoutHeight;
    public MainPage()
	{
		InitializeComponent();
        BindingContext = this;

        // Default values on boot
        entScreenshotsFolder.Text = string.Empty;
        chbMove.IsChecked = false;
        entScreenshotsMoveFolder.Text = string.Empty;
        entScreenshotsMoveFolder.IsEnabled = chbMove.IsChecked;
        btnScreenshotMoveBrowse.IsEnabled = chbMove.IsChecked;
        pickerMonth.SelectedIndex = 2;
        chbCleanEmpty.IsChecked = true;
        chbConvertToWebp.IsChecked = false;
        StartBtn.IsEnabled = false;
        
    }

    public static ICommand OpenURL => new Command<string>(async (url) => await Launcher.OpenAsync(url));
    
    // Screenshot folder handler
    private async void OnSCBrowseClicked(object sender, EventArgs e)
	{
        await Functions.BrowseFolder(entScreenshotsFolder);
#if DEBUG
        System.Diagnostics.Debug.WriteLine($"entScreenshotsFolder: {entScreenshotsFolder.Text}");
#endif
        if(entScreenshotsFolder.Text.Length > 0)
        {
            StartBtn.IsEnabled = true;
        }
    }

    // Checkboxes handlers
    async void MoveIsChecked(object sender, CheckedChangedEventArgs e)
    {
        
        entScreenshotsMoveFolder.IsEnabled = chbMove.IsChecked;
        btnScreenshotMoveBrowse.IsEnabled = chbMove.IsChecked;
#if DEBUG
        System.Diagnostics.Debug.WriteLine($"chbMove: {chbMove.IsChecked}");
#endif
    }

    void DayIsChecked(object sender, CheckedChangedEventArgs e)
    {
#if DEBUG
        System.Diagnostics.Debug.WriteLine($"chbDay: {chbDay.IsChecked}");
#endif
    }

    void CleanEmptyIsChecked(object sender, CheckedChangedEventArgs e)
    {
#if DEBUG
        System.Diagnostics.Debug.WriteLine($"chbCleanEmpty: {chbCleanEmpty.IsChecked}");
#endif
    }

    void ConvertToWebpIsChecked(object sender, CheckedChangedEventArgs e)
    {
#if DEBUG
        System.Diagnostics.Debug.WriteLine($"chbConvertToWebp: {chbConvertToWebp.IsChecked}");
#endif
    }

    // Bind labels to checkboxes
    private void LblMoveClicked(object sender, EventArgs e)
    {
        chbMove.IsChecked = !chbMove.IsChecked;
    }

    private void LblDayClicked(object sender, EventArgs e)
    {
        chbDay.IsChecked = !chbDay.IsChecked;
    }

    private void LblCleanEmptyClicked(object sender, EventArgs e)
    {
        chbCleanEmpty.IsChecked = !chbCleanEmpty.IsChecked;
    }

    private void LblConvertToWebpClicked(object sender, EventArgs e)
    {
        chbConvertToWebp.IsChecked = !chbConvertToWebp.IsChecked;
    }

    // Move-to folder handler
    private async void OnSCMoveBrowseClicked(object sender, EventArgs e)
    {
        await Functions.BrowseFolder(entScreenshotsMoveFolder);
#if DEBUG
        System.Diagnostics.Debug.WriteLine($"entScreenshotsMoveFolder: {entScreenshotsMoveFolder.Text}");
#endif
    }

    // Dropmenu handler
    public int monthNotation;
    public void OnPickerSelectedIndexChanged(object sender, EventArgs e)
    {
        var picker = (Picker)sender;
        int selectedIndex = picker.SelectedIndex;

        if (selectedIndex != -1)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"Picker value: {selectedIndex}");
#endif
            monthNotation = selectedIndex;
        }
    }

    private async void OnStartClicked(object sender, EventArgs e)
    {
        var screenShotsFolder = entScreenshotsFolder.Text;
        bool sortByDay = chbDay.IsChecked;
        bool moveFolders = chbMove.IsChecked;
        var moveToFolder = entScreenshotsMoveFolder.Text;
        bool cleanUp = chbCleanEmpty.IsChecked;
        bool convertToWebp = chbConvertToWebp.IsChecked;
        
        StartBtn.IsEnabled = false;
        StartBtn.IsVisible = false;
        activityInd.IsRunning = true;

        await Functions.Sort(screenShotsFolder, moveFolders, moveToFolder, monthNotation, sortByDay, cleanUp, convertToWebp);

        StartBtn.IsEnabled = true;
        StartBtn.IsVisible = true;
        activityInd.IsRunning = false;

    }
}