# AI-Powered File Search in .NET MAUI TreeView

This sample demonstrates AI-powered semantic file search with the Syncfusion .NET MAUI TreeView. It uses Azure OpenAI to understand natural-language queries, find relevant files from their metadata, and highlight the matching results in a hierarchical file view.

## Overview

Searching a large collection of files by name alone can make relevant information difficult to find. This sample evaluates a query against file names, folder paths, keywords, file types, and created or modified dates to identify and rank relevant files.

The results are displayed in the Syncfusion .NET MAUI TreeView. Matching files are highlighted, and a result count helps users see how many files matched their search.

## Key Features

   - Syncfusion .NET MAUI TreeView integration
   - Azure OpenAI-powered semantic search
   - Natural-language file queries
   - Search across file and folder metadata
   - Ranked file results with matching paths highlighted
   - Hierarchical file and folder display
   - Search result count and clear-search action
   - Cross-platform .NET MAUI application

## Sample

```xaml
<core:SfTextInputLayout Hint=""
                        ContainerType="Outlined">
    <Entry Text="{Binding SearchText}"
           Placeholder="Search files..." />
</core:SfTextInputLayout>

<treeview:SfTreeView ItemsSource="{Binding SalesFileNodes}"
                     ChildPropertyName="SubFiles"
                     ItemTemplateContextType="Node"
                     AutoExpandMode="None"
                     ExpandActionTarget="Expander" />
```

## Output
![AI File Search TreeView](ai-file-search-treeview.gif)

## Requirements to run the demo

To run the demo, refer to https://help.syncfusion.com/maui/system-requirements

## Prerequisites

Before running the sample, ensure that you have:

- Visual Studio with .NET MAUI workload installed
- .NET 10 SDK or later
- Azure OpenAI resource and chat deployment
- Azure OpenAI endpoint, deployment name, and API key configured in `AIFileSearchTreeView/Services/SemanticSearchService.cs`
- Valid Syncfusion license key

## Troubleshooting

### Path too long exception

If you encounter a "Path Too Long" exception while building the project, close Visual Studio, rename the repository folder to a shorter name, and rebuild the project.

## License

Syncfusion has no liability for any damage or consequence that may arise from using or viewing the samples. The samples are for demonstrative purposes. If you choose to use or access the samples, you agree to not hold Syncfusion liable, in any form, for any damage related to use, for accessing, or viewing the samples. By accessing, viewing, or seeing the samples, you acknowledge and agree Syncfusion's samples will not allow you seek injunctive relief in any form for any claim related to the sample. If you do not agree to this, do not view, access, utilize, or otherwise do anything with Syncfusion's samples.
