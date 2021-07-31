namespace Compendium
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Security.Principal;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Forms;
    using System.Windows.Media;
    using Compendium.Directory;
    using Compendium.IconBuilders;
    using Compendium.Models;
    using Microsoft.Extensions.Configuration;
    using Clipboard = System.Windows.Forms.Clipboard;
    using MessageBox = System.Windows.Forms.MessageBox;
    using Path = System.IO.Path;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DirectoryHandler handler;
        private IconBuilder iconBuilder;
        private DesktopIniDistributor desktopIniDistributor;
        FileGridItem currentlySelected;
        FileGridItem currentlySelectedBackup;
        private string iconName;
        public bool isModified { get { return _isModified;  }
            set
            {
                _isModified = value;
                SaveChangesButton.IsEnabled = value;
                DiscardChangesButton.IsEnabled = value;
            }
        }
        private bool _isModified;
        private string loadDirectory;
        private List<System.Drawing.Color> colorList;
        private Bitmap currentPreviewIcon;

        public MainWindow()
        {
            InitializeComponent();
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(System.IO.Directory.GetCurrentDirectory(), "appsettings.json"))
                .Build();
            var lister = new DirectoryLister();
            this.iconName = config.GetSection("IconName").Value;
            var distributor = new DesktopIniDistributor(this.iconName);
            this.handler = new DirectoryHandler(lister, distributor);

            var folderIconFile = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "icon.png");
            this.iconBuilder = new IconBuilder(new Bitmap(folderIconFile));

            this.desktopIniDistributor = new DesktopIniDistributor(this.iconName);

            var defaultLoadDirectory = config.GetSection("DefaultLoadFileDirectory").Value;
            InitFileGrid();
            InitColorLabels();
            if (defaultLoadDirectory.Length != 0 && System.IO.Directory.Exists(defaultLoadDirectory))
            {
                this.loadDirectory = defaultLoadDirectory;
                RefreshGridView();
            }
            isModified = false;

            if(!this.IsAdministrator())
            {
                MessageBox.Show("This application instance doesn't have Admin access and may (will definitely) fail at saving new thumbnails",
                    "WARNING",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void InitColorLabels()
        {
            this.colorList = new List<System.Drawing.Color>()
            {
                System.Drawing.Color.Gray,
                System.Drawing.Color.Red,
                System.Drawing.Color.Blue,
                System.Drawing.Color.Yellow,
                System.Drawing.Color.AntiqueWhite,
                System.Drawing.Color.LightBlue,
                System.Drawing.Color.Black,
                System.Drawing.Color.Violet
            };
            ColorLabelList.ItemsSource = this.colorList;
            ColorLabelList.SelectedIndex = 0;
        }

        private void InitFileGrid()
        {
            FrameworkElementFactory factory = new FrameworkElementFactory(typeof(System.Windows.Controls.Image));
            System.Windows.Data.Binding bind = new System.Windows.Data.Binding("Image");
            factory.SetValue(System.Windows.Controls.Image.SourceProperty, bind);
            DataTemplate cellTemplate = new DataTemplate() { VisualTree = factory };

            var col1 = new DataGridTextColumn();
            var col2 = new DataGridTemplateColumn();
            var col3 = new DataGridTextColumn();
            col1.Header = "";
            col1.Binding = new System.Windows.Data.Binding("Index");
            col2.Header = "";
            col2.CellTemplate = cellTemplate;
            col2.MaxWidth = 128;
            col3.Header = "Title";
            col3.Binding = new System.Windows.Data.Binding("Title");
            col3.MaxWidth = 480;
            var style = new Style(typeof(TextBlock));
            style.Setters.Add(new Setter(TextBlock.TextWrappingProperty, TextWrapping.Wrap));
            col3.ElementStyle = style;
            FileGrid.Columns.Clear();
            FileGrid.Columns.Add(col1);
            FileGrid.Columns.Add(col2);
            FileGrid.Columns.Add(col3);
            FileGrid.ItemsSource = this.handler.list.items;
        }

        private void Open_Directory_Click(object sender, RoutedEventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    this.loadDirectory = fbd.SelectedPath;
                    RefreshGridView();
                }
            }
        }

        private void RefreshGridView()
        {
            string[] files = System.IO.Directory.GetDirectories(this.loadDirectory);
            var items = new List<FileGridItem>();
            var i = 1;
            this.currentlySelected = null;
            foreach (var file in files)
            {
                var item = new FileGridItem()
                {
                    Index = i++,
                    Title = Path.GetFileName(file),
                    ImageFile = Path.Combine(file, this.iconName + ".ico"),
                };
                items.Add(item);
            }
            this.handler.list.SetItems(items);

            FileGrid.ItemsSource = this.handler.list.items;
            this.checkSelectionState();
        }

        private void updateEditUI()
        {
            if(this.currentlySelected != null)
            {
                TitleTextBox.Text = this.currentlySelected.Title;
                IconPreview.Source = this.currentlySelected.Image;
            }
            else
            {
                TitleTextBox.Text = string.Empty;
                IconPreview.Source = null;
            }
        }

        private void FileGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = FileGrid.Items.IndexOf(FileGrid.CurrentItem);
            if(index == -1)
            {
                return;
            }
            this.currentlySelected = new FileGridItem()
            {
                Image = this.handler.list.items[index].Image,
                Index = this.handler.list.items[index].Index,
                Title = this.handler.list.items[index].Title,
                ImageFile = this.handler.list.items[index].ImageFile,
            };
            this.currentlySelectedBackup = new FileGridItem()
            {
                Image = this.handler.list.items[index].Image,
                Index = this.handler.list.items[index].Index,
                Title = this.handler.list.items[index].Title,
                ImageFile = this.handler.list.items[index].ImageFile,
            };
            isModified = false;
            this.checkSelectionState();
            updateEditUI();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(this.currentlySelected != null)
            {
                isModified = this.currentlySelected.Title != TitleTextBox.Text;

                this.currentlySelected.Title = TitleTextBox.Text;
            }
        }

        private void checkSelectionState()
        {
            if(this.currentlySelected == null)
            {
                TitleTextBox.IsEnabled = false;
                ColorLabelList.IsEnabled = false;
                NewIconFromFileButton.IsEnabled = false;
                NewIconFromClipboardButton.IsEnabled = false;
                OpenFolderButton.IsEnabled = false;
                SaveChangesButton.IsEnabled = false;
                DiscardChangesButton.IsEnabled = false;
            }
            else
            {
                TitleTextBox.IsEnabled = true;
                ColorLabelList.IsEnabled = true;
                NewIconFromFileButton.IsEnabled = true;
                NewIconFromClipboardButton.IsEnabled = true;
                OpenFolderButton.IsEnabled = true;
                SaveChangesButton.IsEnabled = true;
                DiscardChangesButton.IsEnabled = true;
            }
        }

        private void DiscardChangesButton_Click(object sender, RoutedEventArgs e)
        {
            this.currentlySelected = new FileGridItem()
            {
                Image = this.currentlySelectedBackup.Image,
                Index = this.currentlySelectedBackup.Index,
                Title = this.currentlySelectedBackup.Title,
                ImageFile = this.currentlySelectedBackup.ImageFile,
            };
            this.checkSelectionState();
            this.isModified = false;
            this.updateEditUI();
        }

        private void NewIconFromClipboardButton_Click(object sender, RoutedEventArgs e)
        {
            if (Clipboard.ContainsImage())
            {
                var clipboardImg = Clipboard.GetImage();
                var colorScheme = this.colorList[ColorLabelList.SelectedIndex];
                this.currentPreviewIcon = new Bitmap(clipboardImg);
                CreatePreviewIcon(colorScheme, this.currentPreviewIcon);
            }
        }

        private void CreatePreviewIcon(System.Drawing.Color colorScheme, Bitmap bitmap)
        {
            this.iconBuilder.DrawIcon(bitmap, colorScheme);
            this.currentlySelected.ImageFile = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "temp.ico");
            var bitmapFile = DirectoryLister.toBitmap(System.IO.File.ReadAllBytes(this.currentlySelected.ImageFile));
            this.currentlySelected.Image = bitmapFile;
            this.updateEditUI();
            isModified = true;
        }

        private void NewIconFromFileButton_Click(object sender, RoutedEventArgs e)
        {
            using (var fbd = new OpenFileDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.FileName))
                {
                    var colorScheme = this.colorList[ColorLabelList.SelectedIndex];
                    this.currentPreviewIcon = new Bitmap(fbd.FileName);
                    CreatePreviewIcon(colorScheme, this.currentPreviewIcon);
                }
            }
        }

        private void SaveChangesButton_Click(object sender, RoutedEventArgs e)
        {
            if(this.currentlySelected.ImageFile != this.currentlySelectedBackup.ImageFile && Path.GetFileName(this.currentlySelected.ImageFile) == "temp.ico")
            {
                var source = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "temp.ico");
                var destination = Path.Combine(this.loadDirectory, this.currentlySelected.Title, this.iconName + ".ico");
                System.IO.File.Copy(source, destination, true);
            }

            if(this.currentlySelected.Title != this.currentlySelectedBackup.Title)
            {
                var source = Path.Combine(this.loadDirectory, this.currentlySelectedBackup.Title);
                var destination = Path.Combine(this.loadDirectory, this.currentlySelected.Title);
                System.IO.Directory.Move(source, destination);
            }

            if(!System.IO.File.Exists(Path.Combine(this.loadDirectory, this.currentlySelected.Title, "desktop.ini")))
            {
                this.desktopIniDistributor.SaveDesktopIniToDirectory(Path.Combine(this.loadDirectory, this.currentlySelected.Title));
            }
            RefreshGridView();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshGridView();
        }

        private void ColorLabelList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(this.currentPreviewIcon != null)
            {
                var colorScheme = this.colorList[ColorLabelList.SelectedIndex];
                this.CreatePreviewIcon(colorScheme, this.currentPreviewIcon);
            }
        }

        private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.currentlySelected != null)
            {
                var destination = Path.Combine(this.loadDirectory, this.currentlySelected.Title);
                Process.Start("explorer.exe", destination);
            }
        }

        private bool IsAdministrator()
        {
            bool isElevated = false;
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            return isElevated;
        }
    }
}
