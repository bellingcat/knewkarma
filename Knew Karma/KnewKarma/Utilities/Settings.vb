﻿Imports System.Globalization
Imports System.IO
Imports System.Security.Principal
Imports Newtonsoft.Json

Public Class SettingsManager
    ''' <summary>
    ''' Represents the Dark Mode property.
    ''' Indicates whether the dark mode is enabled or disabled.
    ''' </summary>
    Public Property DarkMode As Boolean

    ''' <summary>
    ''' Represents the SaveToJson property.
    ''' Indicates whether the application will save data to JSON files.
    ''' </summary>
    Public Property SaveToJson As Boolean

    ''' <summary>
    ''' Represents the SaveToCsv property.
    ''' Indicates whether the application will save data to CSV files.
    ''' </summary>
    Public Property SaveToCsv As Boolean

    ''' <summary>
    ''' Contains the color settings for different visual themes (e.g., Dark, Light).
    ''' The outer dictionary key represents the theme (Dark, Light),
    ''' and the inner dictionary contains the color settings.
    ''' </summary>
    Public Property ColorSettings As Dictionary(Of String, Dictionary(Of String, String))

    ''' <summary>
    ''' Represents the path where the settings file is stored.
    ''' </summary>
    Private ReadOnly settingsFilePath As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Knew Karma", "settings.json")

    ''' <summary>
    ''' Loads application settings from the 'settings.json' file.
    ''' If the settings file doesn't exist, it creates a new file with default settings.
    ''' </summary>
    Public Sub LoadSettings()
        If File.Exists(settingsFilePath) Then
            Dim json As String = File.ReadAllText(settingsFilePath)
            Dim settings = JsonConvert.DeserializeObject(Of SettingsManager)(json)

            DarkMode = settings.DarkMode
            SaveToJson = settings.SaveToJson
            SaveToCsv = settings.SaveToCsv
            ColorSettings = settings.ColorSettings

            Main.ToJSONToolStripMenuItem.Checked = settings.SaveToJson
            Main.ToCSVToolStripMenuItem.Checked = settings.SaveToCsv
        Else
            ' Settings file does not exist
            Dim defaultSettings = New SettingsManager With {
                .DarkMode = False,
                .SaveToCsv = False,
                .SaveToJson = False,
                .ColorSettings = New Dictionary(Of String, Dictionary(Of String, String))() From {
                    {"Dark", New Dictionary(Of String, String)() From {
                        {"MainBackgroundColor", "#FF1A1A1A"},
                        {"PrimaryTextColor", "#FFFFFFFF"},
                        {"SecondaryTextColor", "#FFFF4500"},
                        {"InputFieldBackgroundColor", "#FF2E2E2E"},
                        {"MenuItemMouseEnterColor", "#FF000000"}
                    }},
                    {"Light", New Dictionary(Of String, String)() From {
                         {"MainBackgroundColor", "#FFF0F0F0"},
                         {"PrimaryTextColor", "#FF121212"},
                         {"SecondaryTextColor", "#FF5F99CF"},
                         {"InputFieldBackgroundColor", "#FFFFFAFA"},
                         {"MenuItemMouseEnterColor", "#FFFFFFFF"}
                    }}
                }
            }

            ' Write settings to jsonOutput file
            Dim jsonOutput = JsonConvert.SerializeObject(defaultSettings)
            File.WriteAllText(settingsFilePath, jsonOutput)

            SaveToJson = False
            SaveToCsv = False
            ColorSettings = defaultSettings.ColorSettings
            Main.ToJSONToolStripMenuItem.Checked = False
            Main.ToCSVToolStripMenuItem.Checked = False

            ' Check the system settings to see if dark mode is enabled/disabled
            If CoreUtils.IsSystemDarkTheme() Then
                ' If dark mode is enabled in the system settings, update the program's dark mode settings: 
                ' set DarkMode to True, check the DarkModeEnable menu item, and uncheck the DarkModeDisable menu item.
                DarkMode = True
                Main.DarkModeEnableToolStripMenuItem.Checked = True
                Main.DarkModeDisableToolStripMenuItem.Checked = False
            Else
                ' If dark mode is not enabled in the system settings, update the program's dark mode settings:
                ' set DarkMode to False, uncheck the DarkModeEnable menu item, and check the DarkModeDisable menu item.
                DarkMode = False
                Main.DarkModeEnableToolStripMenuItem.Checked = False
                Main.DarkModeDisableToolStripMenuItem.Checked = True
            End If
        End If
    End Sub

    ''' <summary>
    ''' Retrieves application settings as key-value pairs.
    ''' </summary>
    ''' <returns>A Dictionary containing the names and values of all settings.</returns>
    Private Function GetSettings() As Dictionary(Of String, Object)
        Dim settings As New Dictionary(Of String, Object)()

        If File.Exists(settingsFilePath) Then
            Dim json As String = File.ReadAllText(settingsFilePath)
            settings = JsonConvert.DeserializeObject(Of Dictionary(Of String, Object))(json)
        End If

        Return settings
    End Function

    ''' <summary>
    ''' Saves the provided settings to a JSON file with a read-only attribute.
    ''' </summary>
    ''' <param name="settings">An instance of the SettingsManager containing the configurations to be saved.</param>
    Private Sub SaveSettings(settings)
        ' Prepare the file to be written by ensuring it's not read-only
        Dim fileInfo As New FileInfo(settingsFilePath)
        If fileInfo.Exists AndAlso fileInfo.IsReadOnly Then
            fileInfo.IsReadOnly = False
        End If

        ' Serialize and write the data
        Dim jsonOutput = JsonConvert.SerializeObject(settings)
        File.WriteAllText(settingsFilePath, jsonOutput)

        ' Reset the read-only attribute
        fileInfo.Refresh() ' This ensures that we're working with the latest file info
        fileInfo.IsReadOnly = True
    End Sub

    ''' <summary>
    ''' Applies the current settings to the application's interface. This includes
    ''' toggling SaveToJson, SaveToCsv, and applying the visual theme based on the Dark Mode setting.
    ''' </summary>
    Public Sub ApplySettings()
        ' Retrieve the current settings
        Dim settings As Dictionary(Of String, Object) = GetSettings()
        ' Apply the SaveToJson setting to the menu item checkbox
        Main.ToJSONToolStripMenuItem.Checked = Me.SaveToJson

        ' Apply the SaveToCsv setting to the menu item checkbox
        Main.ToCSVToolStripMenuItem.Checked = Me.SaveToCsv

        ' Use ColorSettings property directly
        Dim colorSettings As Dictionary(Of String, Dictionary(Of String, String)) = Me.ColorSettings

        ' Apply the color scheme based on the Dark Mode setting
        ApplyColorScheme(isDarkMode:=CBool(settings("DarkMode")), colorSettings:=colorSettings)
    End Sub

    ''' <summary>
    ''' Applies the color scheme based on the given Dark Mode setting. 
    ''' Colors are defined in a mapping for easier maintenance and flexibility.
    ''' </summary>
    ''' <param name="isDarkMode">Indicates whether Dark Mode is enabled.</param>
    Public Shared Sub ApplyColorScheme(ByVal isDarkMode As Boolean, ByVal colorSettings As Dictionary(Of String, Dictionary(Of String, String)))
        Dim color As New Dictionary(Of String, Color)

        Dim mode As String = If(isDarkMode, "Dark", "Light")
        Dim modeColors As Dictionary(Of String, String) = colorSettings(mode)

        For Each colorName As String In modeColors.Keys
            color(colorName) = ColorTranslator.FromHtml(modeColors(colorName))
        Next

        SetUIColors(color:=color)

        If isDarkMode Then
            Main.DarkModeEnableToolStripMenuItem.Text = "Enabled"
            Main.DarkModeDisableToolStripMenuItem.Text = "Disable"
            Main.DarkModeEnableToolStripMenuItem.Checked = True
            Main.DarkModeDisableToolStripMenuItem.Checked = False
        Else
            Main.DarkModeEnableToolStripMenuItem.Text = "Enable"
            Main.DarkModeDisableToolStripMenuItem.Text = "Disabled"
            Main.DarkModeEnableToolStripMenuItem.Checked = False
            Main.DarkModeDisableToolStripMenuItem.Checked = True
        End If
    End Sub

    ''' <summary>
    ''' Applies the specified color settings to the UI components.
    ''' </summary>
    ''' <param name="color">A dictionary mapping color names to Color objects.</param>
    Private Shared Sub SetUIColors(ByVal color As Dictionary(Of String, Color))
        Main.TreeView1.BackColor = color("InputFieldBackgroundColor")
        Main.TreeView1.ForeColor = color("PrimaryTextColor")
        Main.TreeView1.LineColor = color("SecondaryTextColor")

        ''' <summary>
        ''' Apply colors to data grid view controls
        ''' </summary>
        Dim dataGridViews As New List(Of DataGridView) From {
            UserProfile.DataGridViewUserProfile,
            UserProfile.DataGridViewUserSubreddit,
            Comments.DataGridViewComments,
            MiscData.DataGridViewProfile,
            Posts.DataGridViewPosts
        }
        For Each datagridview In dataGridViews
            datagridview.BackColor = color("MainBackgroundColor")
            datagridview.ForeColor = color("PrimaryTextColor")
        Next

        ''' <summary>
        ''' Apply colors to data grid view cells
        ''' </summary>
        Dim cellStyles As New List(Of DataGridViewCellStyle) From {
            MiscData.DataGridViewProfile.AlternatingRowsDefaultCellStyle,
            MiscData.DataGridViewProfile.DefaultCellStyle,
            UserProfile.DataGridViewUserProfile.AlternatingRowsDefaultCellStyle,
            UserProfile.DataGridViewUserProfile.DefaultCellStyle,
            UserProfile.DataGridViewUserSubreddit.AlternatingRowsDefaultCellStyle,
            UserProfile.DataGridViewUserSubreddit.DefaultCellStyle,
            Posts.DataGridViewPosts.AlternatingRowsDefaultCellStyle,
            Posts.DataGridViewPosts.RowHeadersDefaultCellStyle,
            Posts.DataGridViewPosts.ColumnHeadersDefaultCellStyle,
            Posts.DataGridViewPosts.DefaultCellStyle,
            Comments.DataGridViewComments.AlternatingRowsDefaultCellStyle,
            Comments.DataGridViewComments.RowHeadersDefaultCellStyle,
            Comments.DataGridViewComments.ColumnHeadersDefaultCellStyle,
            Comments.DataGridViewComments.DefaultCellStyle
            }
        For Each cellStyle In cellStyles
            cellStyle.BackColor = color("InputFieldBackgroundColor")
            cellStyle.ForeColor = color("PrimaryTextColor")
        Next

        ''' <summary>
        ''' Apply colors to tab pages
        ''' </summary>
        Dim tabpages As New List(Of TabPage) From {
            UserProfile.TabPage1,
            UserProfile.TabPage2
        }
        For Each tabpage In tabpages
            tabpage.BackColor = color("InputFieldBackgroundColor")
            tabpage.ForeColor = color("PrimaryTextColor")
        Next

        ''' <summary>
        ''' Apply colors to Forms
        ''' </summary>
        Dim forms As New List(Of Form) From {
            Main,
            About,
            Comments,
            MiscData,
            Posts
            }
        For Each form In forms
            form.BackColor = color("MainBackgroundColor")
            form.ForeColor = color("PrimaryTextColor")
        Next

        ''' <summary>
        ''' Apply colors to buttons
        ''' </summary>
        Dim buttons As New List(Of Button) From {
            Main.ButtonFetchFrontPageData,
            Main.ButtonFetchPostListings,
            Main.ButtonFetchSubredditData,
            Main.ButtonFetchUserData,
            Main.ButtonSearch,
            About.ButtonViewLicense,
            About.ButtonGetUpdates
        }
        ' Main.ButtonFetchPostData,
        For Each button In buttons
            button.BackColor = color("InputFieldBackgroundColor")
            button.ForeColor = color("PrimaryTextColor")
        Next

        ''' <summary>
        ''' Apply colors to Label controls
        ''' </summary>
        Dim labels As New List(Of Label) From {
            Main.LabelPostListingsLimit,
            Main.LabelPostListingsListing,
            Main.LabelFrontPageDataLimit,
            Main.LabelFrontPageDataListing,
            Main.LabelSearchResultsLimit,
            Main.LabelSearchResultsListing,
            Main.LabelUserPostsListing,
            Main.LabelUserDataLimit,
            Main.LabelSubredditPostsListing,
            Main.LabelSubredditDataLimit,
            About.LabelProgramFirstName,
            Main.Label1,
            Main.Label2,
            Main.Label3,
            Main.Label4,
            Main.Label5
            }
        About.LabelProgramLastName.ForeColor = color("SecondaryTextColor")
        For Each label In labels
            label.ForeColor = color("PrimaryTextColor")
        Next

        ''' <summary>
        ''' Apply colors to RadioButton controls
        ''' </summary>
        Dim radioButtons As New List(Of RadioButton) From {
            Main.RadioButtonBest,
            Main.RadioButtonRising,
            Main.RadioButtonPopular,
            Main.RadioButtonControversial,
            Main.RadioButtonUserProfile,
            Main.RadioButtonUserPosts,
            Main.RadioButtonUserComments,
            Main.RadioButtonSubredditProfile,
            Main.RadioButtonSubredditPosts
            }
        ' Main.RadioButtonPostProfile,
        ' Main.RadioButtonPostComments,
        For Each radioButton In radioButtons
            radioButton.BackColor = color("MainBackgroundColor")
            radioButton.ForeColor = color("PrimaryTextColor")
        Next

        ''' <summary>
        ''' Apply colors to TextBox controls
        ''' </summary>
        Dim textBoxes As New List(Of TextBox) From {
            Main.TextBoxUsername,
            Main.TextBoxQuery,
            Main.TextBoxSubreddit
            }

        For Each textBox In textBoxes
            textBox.BackColor = color("InputFieldBackgroundColor")
            textBox.ForeColor = color("PrimaryTextColor")
        Next

        ''' <summary>
        ''' Apply colors to NumericUpDown controls
        ''' </summary>
        Dim numericUpDowns As New List(Of NumericUpDown) From {
            Main.NumericUpDownPostListingsLimit,
            Main.NumericUpDownFrontPageDataLimit,
            Main.NumericUpDownSubredditDataLimit,
            Main.NumericUpDownUserDataLimit,
            Main.NumericUpDownSearchResultLimit
            }
        For Each numericUpDown In numericUpDowns
            numericUpDown.BackColor = color("InputFieldBackgroundColor")
            numericUpDown.ForeColor = color("PrimaryTextColor")
        Next

        ''' <summary>
        ''' Apply colors to ComboBox controls
        ''' </summary>
        Dim comboBoxes As New List(Of ComboBox) From {
            Main.ComboBoxPostListingsListing,
            Main.ComboBoxFrontPageDataListing,
            Main.ComboBoxSubredditPostsListing,
            Main.ComboBoxUserPostsListing,
            Main.ComboBoxSearchResultListing
            }
        For Each comboBox In comboBoxes
            comboBox.BackColor = color("InputFieldBackgroundColor")
            comboBox.ForeColor = color("PrimaryTextColor")
        Next

        ''' <summary>
        ''' Apply colors to GroupBox controls
        ''' </summary>
        Dim GroupBoxes As New List(Of GroupBox) From {
            Main.GroupBoxPostListings,
            Main.GroupBoxFrontPageDataFiltering,
            Main.GroupBoxSubredditDataFiltering,
            Main.GroupBoxUserDataFiltering,
            Main.GroupBoxSearchResultsFiltering,
            Main.GroupBoxPostListingsFiltering,
            Main.GroupBoxUserData,
            Main.GroupBoxSubredditData
            }
        ' Main.GroupBoxPostDataFiltering,
        ' Main.GroupBoxPostData,
        For Each groupBox In GroupBoxes
            groupBox.BackColor = color("MainBackgroundColor")
            groupBox.ForeColor = color("SecondaryTextColor")
        Next

        ''' <summary>
        ''' Apply colors to ToolStripMenuItem items
        ''' </summary>
        Dim menuItems As New List(Of ToolStripMenuItem) From {
            Main.SettingsToolStripMenuItem,
            Main.DarkModeToolStripMenuItem,
            Main.DarkModeEnableToolStripMenuItem,
            Main.DarkModeDisableToolStripMenuItem,
            Main.SaveDataToolStripMenuItem,
            Main.ToJSONToolStripMenuItem,
            Main.ToCSVToolStripMenuItem,
            Main.AboutToolStripMenuItem,
            Main.ExitToolStripMenuItem
            }
        For Each menuItem In menuItems
            menuItem.BackColor = color("MainBackgroundColor")
            menuItem.ForeColor = color("PrimaryTextColor")
        Next

        ''' <summary>
        ''' Apply mouse enter/mouse leave colors to ToolStripMenuItem and ContextMenuStrip controls
        ''' </summary>
        Dim toolStripItems As New List(Of ToolStripMenuItem) From {
            Main.SettingsToolStripMenuItem,
            Main.DarkModeToolStripMenuItem,
            Main.SaveDataToolStripMenuItem
        }
        ' Iterate over the individual menu items
        For Each toolStripItem In toolStripItems
            For Each item As ToolStripMenuItem In toolStripItem.DropDownItems
                ' Add handlers for MouseEnter and MouseLeave events
                AddHandler item.MouseEnter, Sub(sender As Object, e As EventArgs)
                                                Main.OnMenuItemMouseEnter(sender:=sender, e:=e, color:=color)
                                            End Sub
                AddHandler item.MouseLeave, Sub(sender As Object, e As EventArgs)
                                                Main.OnMenuItemMouseLeave(sender:=sender, e:=e, color:=color)
                                            End Sub
            Next
        Next

        ' Iterate over the context menus and their items
        For Each item As ToolStripMenuItem In Main.ContextMenuStripRightClick.Items
            ' Add handlers for MouseEnter and MouseLeave events
            AddHandler item.MouseEnter, Sub(sender As Object, e As EventArgs)
                                            Main.OnMenuItemMouseEnter(sender:=sender, e:=e, color:=color)
                                        End Sub
            AddHandler item.MouseLeave, Sub(sender As Object, e As EventArgs)
                                            Main.OnMenuItemMouseLeave(sender:=sender, e:=e, color:=color)
                                        End Sub
        Next
    End Sub

    ''' <summary>
    ''' Toggles specific settings on or off based on the provided parameters.
    ''' </summary>
    ''' <param name="enabled">A Boolean indicating if the setting option should be enabled or not.</param>
    ''' <param name="saveTo">A String specifying the type of setting to toggle ('json', 'csv', or 'darkmode').</param>
    Public Sub ToggleSettings(enabled As Boolean, saveTo As String)
        ' Read the existing settings from the settings file
        Dim json As String = File.ReadAllText(settingsFilePath)
        Dim settings As SettingsManager = JsonConvert.DeserializeObject(Of SettingsManager)(json)

        ' Update the settings based on the specified saveTo parameter
        If saveTo.ToLower(CultureInfo.InvariantCulture) = "json" Then
            settings.SaveToJson = enabled
            Me.SaveToJson = enabled ' Update the current instance property
        ElseIf saveTo.ToLower(CultureInfo.InvariantCulture) = "csv" Then
            settings.SaveToCsv = enabled
            Me.SaveToCsv = enabled ' Update the current instance property
        ElseIf saveTo.ToLower(CultureInfo.InvariantCulture) = "darkmode" Then
            settings.DarkMode = enabled
            Me.DarkMode = enabled ' Update the current instance property
        End If

        ' Save the updated settings back to the settings file
        SaveSettings(settings:=settings)
        ' Apply the updated settings to the application
        ApplySettings()
    End Sub
End Class