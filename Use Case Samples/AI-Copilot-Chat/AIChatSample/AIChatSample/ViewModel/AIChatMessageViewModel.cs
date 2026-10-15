using Syncfusion.Maui.AIAssistView;
using Syncfusion.Office.Markdown;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;

namespace AIChatSample.ViewModel
{
    public class AIChatMessageViewModel : INotifyPropertyChanged
    {

        private ObservableCollection<IAssistItem> assistItems;

        public AIChatMessageViewModel()
        {
            assistItems = new ObservableCollection<IAssistItem>();

            GenerateStaticMessages();
        }

        public ObservableCollection<IAssistItem> AssistItems
        {
            get => assistItems;
            set
            {
                assistItems = value;
                OnPropertyChanged(nameof(AssistItems));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void GenerateStaticMessages()
        {
            const string requirements = "Requirements:\n- Strictly use existing Design Kit assets, components, design tokens, icons, color styles, typography, spacing, elevations, and interaction patterns.\n- Do not create new components unless absolutely necessary.";
            const string securityNote = "Requirements:\n- Strictly use existing Design Kit assets, components, design tokens, icons, color styles, typography, spacing, elevations, and interaction patterns.\n- Do not create new components unless absolutely necessary.\n\nⓘ  Note: Security is a top priority for this implementation.";
            var requirementsRequest = new AssistItem
            {
                Text = requirements,
                IsRequested = true
            };

            var followUpRequest = new AssistItem
            {
                Text = "Do not create new components unless absolutely necessary.",
                IsRequested = true
            };

            this.AssistItems.Add(requirementsRequest);
            this.AssistItems.Add(followUpRequest);
            this.AssistItems.Add(new AssistItem
            {
                Text = securityNote,
            });
            this.AssistItems.Add(new AssistItem
            {
                Text = securityNote,
            });
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class AIChatCodeViewModel : INotifyPropertyChanged
    {

        private ObservableCollection<IAssistItem> assistItems;

        public AIChatCodeViewModel()
        {
            assistItems = new ObservableCollection<IAssistItem>();

            GenerateStaticMessages();
        }

        public ObservableCollection<IAssistItem> AssistItems
        {
            get => assistItems;
            set
            {
                assistItems = value;
                OnPropertyChanged(nameof(AssistItems));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void GenerateStaticMessages()
        {
            const string requirements = "Requirements:\n- Strictly use existing Design Kit assets, components, design tokens, icons, color styles, typography, spacing, elevations, and interaction patterns.\n- Do not create new components unless absolutely necessary.";
            var requirementsRequest = new AssistItem
            {
                Text = requirements,
                IsRequested = true
            };

            var followUpRequest = new AssistItem
            {
                Text = "Do not create new components unless absolutely necessary.",
                IsRequested = true
            };

            this.AssistItems.Add(requirementsRequest);
            this.AssistItems.Add(followUpRequest);
            var markDown = """
                Requirements:
                - Strictly use existing Design Kit assets, components, design tokens, icons, color styles, typography, spacing, elevations, and interaction patterns.
                - Do not create new components unless absolutely necessary.
            
                ```xaml
                <ContentPage
                    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
                    x:Class="SampleApp.MainPage">
            
                    <VerticalStackLayout Padding="20" Spacing="15">
                        <Label
                            Text="Welcome to XAML!"
                            FontSize="24"
                            HorizontalOptions="Center" />
                        <Entry
                            Placeholder="Enter your name" />
                        <Button
                            Text="Submit"
                            Clicked="OnSubmitClicked" />
                    </VerticalStackLayout>
            
                </ContentPage>
                ```
                """;

            this.AssistItems.Add(new AssistItem
            {
                Text = markDown,
            });
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
