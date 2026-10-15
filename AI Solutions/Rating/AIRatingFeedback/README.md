# AI-Powered Feedback in .NET MAUI Rating

This sample demonstrates AI-powered customer support feedback rating in a .NET MAUI application. It uses Azure OpenAI to analyze written feedback and suggest a satisfaction rating displayed with the Syncfusion .NET MAUI Rating control.

## Overview

Customer feedback often describes a support experience in natural language rather than as a numeric score. This sample evaluates the feedback sentiment and aspects such as issue resolution, response time, and helpfulness, then suggests a rating from 0 to 5.

The rating updates as the user enters feedback. A brief typing pause prevents unnecessary AI requests while the user is still writing.

## Key Features

   - Syncfusion .NET MAUI Rating control integration
   - Azure OpenAI-powered feedback analysis
   - Suggested satisfaction rating from 0 to 5
   - Rating updates as feedback is entered
   - Debounced requests while typing
   - Loading indicator while feedback is being analyzed
   - Cross-platform .NET MAUI application

## Sample

```xaml
<rating:SfRating ItemCount="5"
                 ItemSize="32"
                 Value="0"
                 IsReadOnly="True" />

<inputLayout:SfTextInputLayout CharMaxLength="300"
                               ShowCharCount="True">
    <Editor Placeholder="Describe your customer support experience..."
            TextChanged="FeedbackEditor_TextChanged" />
</inputLayout:SfTextInputLayout>
```

## Output
![AI Rating Feedback](AIRatingFeedback.gif)

## Requirements to run the demo

To run the demo, refer to https://help.syncfusion.com/maui/system-requirements

## Prerequisites

Before running the sample, ensure that you have:

- Visual Studio with .NET MAUI workload installed
- .NET 10 SDK or later
- Azure OpenAI resource and chat deployment
- Azure OpenAI endpoint, deployment name, and API key configured in `AIRatingFeedback/Services/OpenAIService.cs`
- Valid Syncfusion license key

## Troubleshooting

### Path too long exception

If you encounter a "Path Too Long" exception while building the project, close Visual Studio, rename the repository folder to a shorter name, and rebuild the project.

## License

Syncfusion has no liability for any damage or consequence that may arise from using or viewing the samples. The samples are for demonstrative purposes. If you choose to use or access the samples, you agree to not hold Syncfusion liable, in any form, for any damage related to use, for accessing, or viewing the samples. By accessing, viewing, or seeing the samples, you acknowledge and agree Syncfusion's samples will not allow you seek injunctive relief in any form for any claim related to the sample. If you do not agree to this, do not view, access, utilize, or otherwise do anything with Syncfusion's samples.
