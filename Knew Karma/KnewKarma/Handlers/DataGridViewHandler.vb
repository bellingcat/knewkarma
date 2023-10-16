﻿Imports Newtonsoft.Json.Linq

Public Class DataGridViewer


    ''' <summary>
    ''' Privately shared instance of ApiHandler to be used by methods in this class for interacting with the GitHub API.
    ''' </summary>
    Private Shared ReadOnly apiHandler As New ApiHandler()


    ''' <summary>
    ''' Sets up the DataGridView by clearing its existing columns and rows and adding new columns.
    ''' </summary>
    ''' <param name="dataGrid">The DataGridView to configure.</param>
    ''' <param name="columnHeaders">A Dictionary containing column keys and their display names.</param>
    Private Shared Sub SetupDataGrid(dataGrid As DataGridView, columnHeaders As Dictionary(Of String, String))
        dataGrid.Rows.Clear()
        dataGrid.Columns.Clear()

        For Each kvp In columnHeaders
            dataGrid.Columns.Add(kvp.Key, kvp.Value)
        Next
    End Sub


    ''' <summary>
    ''' Adds data as a row to the specified DataGridView on a form.
    ''' </summary>
    ''' <param name="data">The JObject containing the data to be added to the datagridview.</param>
    ''' <param name="form">The form containing the DataGridView.</param>
    ''' <param name="dataGridViewName">The name of the DataGridView control to add the row to.</param>
    ''' <param name="RowKeys">A list of keys from the API response that represent the value of each Row.</param>
    ''' <returns>The updated form.</returns>
    Private Shared Function AddToDataGridView(data As JObject, form As Form, dataGridViewName As String, RowKeys As List(Of String)) As Form

        Dim postRowData As New List(Of String)  ' Separate list to hold the values

        ' Loop through each key to fetch the respective data
        For Each RowKey As String In RowKeys
            Dim value As String ' Variable to hold the data

            ' Special handling for the "created" timestamp
            If RowKey = "created" Then
                Dim timestamp As Double = CDbl(data("data")(RowKey))
                value = CoreUtils.ConvertTimestampToDatetime(timestamp)
            Else
                value = If(data("data")(RowKey)?.ToString(), "N/A")
            End If

            ' Add the value to the postRowData list
            postRowData.Add(value)
        Next

        ' Find the DataGridView control by its name and add the row to it
        Dim dgv As DataGridView = CType(form.Controls(dataGridViewName), DataGridView)
        dgv?.Rows.Add(postRowData.ToArray())

        Return form
    End Function

    Public Shared Async Function AsyncLoadListingsPosts(sort As String, limit As Integer) As Task
        Dim postsList As JArray = Nothing
        Dim listing As String = String.Empty

        If Main.RadioButtonBest.Checked Then
            listing = Main.RadioButtonBest.Text
            postsList = Await apiHandler.AsyncGetListingsPosts(sort:=sort, limit:=limit, listing:=listing)
        ElseIf Main.RadioButtonRising.Checked Then
            listing = Main.RadioButtonRising.Text
            postsList = Await apiHandler.AsyncGetListingsPosts(sort:=sort, limit:=limit, listing:=listing)
        ElseIf Main.RadioButtonControversial.Checked Then
            listing = Main.RadioButtonControversial.Text
            postsList = Await apiHandler.AsyncGetListingsPosts(sort:=sort, limit:=limit, listing:=listing)
        ElseIf Main.RadioButtonPopular.Checked Then
            listing = Main.RadioButtonPopular.Text
            postsList = Await apiHandler.AsyncGetListingsPosts(sort:=sort, limit:=limit, listing:=listing)
        End If

        Dim isValid As Boolean = CoreUtils.IsValidData(data:=postsList)
        If isValid Then
            ' Iterate over each post and add its data to the DataGridView.
            For Each post As JObject In postsList
                AddToDataGridView(data:=post, form:=Posts, dataGridViewName:="DataGridViewPosts", RowKeys:=DataGridHelper.PostRowKeys)
            Next
            Posts.Text = $"{sort} {listing} {limit} posts"
            Posts.Show()

            ' Prompt to save data if the conditions are met.
            CoreUtils.PromptSaveData(data:=postsList, title:=$"{listing} posts")
        End If
    End Function

    Public Shared Async Function AsyncLoadPostData(postId As String, postSubreddit As String, sort As String, limit As Integer) As Task
        Dim Data = Await apiHandler.AsyncGetPostData(postId:=postId, postSubreddit:=postSubreddit, sort:=sort, limit:=limit)

        Dim postData As JObject = Data.postData
        Dim commentsList As JArray = Data.postComments

        If Main.RadioButtonPostComments.Checked Then
            PostComments(commentsList:=commentsList)
        End If

    End Function

    ''' <summary>
    ''' Loads the comments of a specific user and updates the DataGridView in the Comments.form.
    ''' </summary>
    ''' <param name="username">The Reddit username for which to fetch comments' data.</param>
    ''' <param name="form">The Comments.object that contains the DataGridView to be updated.</param>
    ''' <returns>A Task representing the asynchronous operation.</returns>
    Private Shared Sub PostComments(commentsList As JArray)
        Dim isValid As Boolean = CoreUtils.IsValidData(data:=commentsList)

        ' If the API data is valid, setup a DataGridView for the Posts.
        If isValid Then
            ' Iterate over each comment and add its data to the DataGridView.
            For Each Comment As JObject In commentsList
                AddToDataGridView(data:=Comment, form:=Comments, dataGridViewName:="DataGridViewComments", RowKeys:=DataGridHelper.CommentRowKeys)
            Next
            'Comments.Text = $"u/{username}'s {sort} {limit} comments"
            Comments.Show()

            ' Prompt to save data if the conditions are met.
            'CoreUtils.PromptSaveData(data:=Comments, title:=$"User ({username}) comments")
        End If
    End Sub


    ''' <summary>
    ''' Asynchronously gets the user profile data and updates the DataGridView in the FormUserProfile form with it.
    ''' </summary>
    ''' <param name="username">The username to get the data for.</param>
    Private Shared Async Function AsyncLoadUserProfile(username As String) As Task
        Dim ProfileData As JObject = Await apiHandler.AsyncGetUserProfile(username:=username)
        Dim IsValid As Boolean = CoreUtils.IsValidData(data:=ProfileData)

        If IsValid Then
            UserProfile.Text = $"User Profile - {username}"


            SetupDataGrid(UserProfile.DataGridViewUserProfile, DataGridHelper.DefaultColumnHeaders)

            ' Loop over each property and populate the DataGridView accordingly.
            For Each KeyValuePair In DataGridHelper.UserProfileHeaderMapping
                Dim key = KeyValuePair.Key
                Dim header = KeyValuePair.Value
                Dim value As String

                ' Special handling for nested properties.
                If key = "subreddit" Then
                    value = If(ProfileData(key)?("title")?.ToString(), "N/A")
                Else
                    value = If(ProfileData(key)?.ToString(), "N/A")
                End If

                ' Add a row with the header and value.
                UserProfile.DataGridViewUserProfile.Rows.Add(New Object() {header, value})
            Next
            UserProfile.Show()
            ' Get the user subreddit data from the user's profile data.
            LoadUserSubreddit(UserSubredditData:=ProfileData)

            ' Prompt to save data if the conditions are met.
            CoreUtils.PromptSaveData(data:=ProfileData, $"User ({username}) profile")
        End If
    End Function


    ''' <summary>
    ''' Asynchronously gets the user subreddit data and updates the DataGridView in the FormUserSubreddit form with it.
    ''' </summary>
    ''' <param name="data">The user subreddit data.</param>
    ''' <param name="form">The UserSubredditeForm to update.</param>
    Private Shared Sub LoadUserSubreddit(UserSubredditData As JObject)



        SetupDataGrid(UserProfile.DataGridViewUserSubreddit, DataGridHelper.DefaultColumnHeaders)

        ' Loop over each property and populate the DataGridView accordingly.
        For Each kvp In DataGridHelper.UserSubredditHeaderMapping
            Dim key = kvp.Key
            Dim header = kvp.Value

            ' Add a row with the header and value.
            UserProfile.DataGridViewUserSubreddit.Rows.Add(New Object() {header, If(UserSubredditData("subreddit")(key)?.ToString(), "N/A")})
        Next
    End Sub

    ''' <summary>
    ''' Asynchronously loads the posts of a specific user and updates the DataGridView in the Posts.form.
    ''' </summary>
    ''' <param name="username">The Reddit username for which to fetch post data.</param>
    ''' <param name="form">The Posts.object that contains the DataGridView to be updated.</param>
    ''' <returns>A Task representing the asynchronous operation.</returns>
    Private Shared Async Function UserPosts(username As String) As Task
        Dim sort As String = Main.ComboBoxUserPostsListing.Text
        Dim limit As Integer = Main.NumericUpDownUserDataLimit.Value


        Dim postsList As JArray = Await apiHandler.AsyncGetUserPosts(username:=username, sort:=sort, limit:=limit)
        Dim isValid As Boolean = CoreUtils.IsValidData(data:=postsList)

        ' If the API data is valid, setup a DataGridView for the Posts.
        If isValid Then
            ' Iterate over each post and add its data to the DataGridView.
            For Each post As JObject In postsList
                AddToDataGridView(data:=post, form:=Posts, dataGridViewName:="DataGridViewPosts", RowKeys:=DataGridHelper.PostRowKeys)
            Next
            Posts.Text = $"u/{username}'s {sort} {limit} posts"
            Posts.Show()

            ' Prompt to save data if the conditions are met.
            CoreUtils.PromptSaveData(data:=postsList, title:=$"User ({username}) posts")
        End If
    End Function


    ''' <summary>
    ''' Asynchronously loads the comments of a specific user and updates the DataGridView in the Comments.form.
    ''' </summary>
    ''' <param name="username">The Reddit username for which to fetch comments' data.</param>
    ''' <param name="form">The Comments.object that contains the DataGridView to be updated.</param>
    ''' <returns>A Task representing the asynchronous operation.</returns>
    Private Shared Async Function UserComments(username As String) As Task
        Dim sort As String = Main.ComboBoxUserPostsListing.Text
        Dim limit As Integer = Main.NumericUpDownUserDataLimit.Value


        Dim commentsList As JArray = Await apiHandler.AsyncGetUserComments(username:=username, sort:=sort, limit:=limit)
        Dim isValid As Boolean = CoreUtils.IsValidData(data:=commentsList)

        ' If the API data is valid, setup a DataGridView for the Posts.
        If isValid Then
            ' Iterate over each comment and add its data to the DataGridView.
            For Each Comment As JObject In commentsList
                AddToDataGridView(data:=Comment, form:=Comments, dataGridViewName:="DataGridViewComments", RowKeys:=DataGridHelper.CommentRowKeys)
            Next
            Comments.Text = $"u/{username}'s {sort} {limit} comments"
            Comments.Show()

            ' Prompt to save data if the conditions are met.
            CoreUtils.PromptSaveData(data:=commentsList, title:=$"User ({username}) comments")
        End If
    End Function

    ''' <summary>
    ''' Asynchronously load a user's data and update either the FormProfile, Posts.or FormUserComments forms 
    ''' depending on which Radio Button is checked.
    ''' </summary>
    ''' <param name="username">The username to fetch data for.</param>
    Public Shared Async Function AsyncLoadUserData(username As String) As Task
        If Main.RadioButtonUserProfile.Checked Then
            Await AsyncLoadUserProfile(username:=username)
        ElseIf Main.RadioButtonUserPosts.Checked Then
            Await UserPosts(username:=username)
        ElseIf Main.RadioButtonUserComments.Checked Then
            Await UserComments(username:=username)
        End If
    End Function

    ''' <summary>
    ''' Asynchronously load a subreddit's profile data and updates the FormProfile form.
    ''' </summary>
    ''' <param name="username">The username to fetch data for.</param>
    Public Shared Async Function SubredditProfile(subreddit As String) As Task
        Dim ProfileData As JObject = Await apiHandler.AsyncGetSubredditProfile(subreddit:=subreddit)
        Dim IsValid As Boolean = CoreUtils.IsValidData(data:=ProfileData)
        If IsValid Then
            MiscData.Text = $"Subreddit Profile - {subreddit}"

            SetupDataGrid(MiscData.DataGridViewProfile, DataGridHelper.DefaultColumnHeaders)

            ' Loop over each property and populate the DataGridView accordingly.
            For Each KeyValuePair In DataGridHelper.SubRedditProfileHeaderMapping
                Dim key = KeyValuePair.Key
                Dim header = KeyValuePair.Value
                ' Add a row with the header and value.
                MiscData.DataGridViewProfile.Rows.Add(New Object() {header, ProfileData(key)})
            Next

            MiscData.Show()

            ' Prompt to save data if the conditions are met.
            CoreUtils.PromptSaveData(data:=ProfileData, title:=$"Subreddit ({subreddit}) profile")

        End If
    End Function


    ''' <summary>
    ''' Asynchronously loads the posts of a specific subreddit and updates the DataGridView in the Posts.form.
    ''' </summary>
    ''' <param name="subreddit">The Subreddit for which to fetch post data.</param>
    ''' <param name="form">The Posts.object that contains the DataGridView to be updated.</param>
    ''' <returns>A Task representing the asynchronous operation.</returns>
    Private Shared Async Function SubredditPosts(subreddit As String) As Task
        Dim sort As String = Main.ComboBoxSubredditPostsListing.Text
        Dim limit As Integer = Main.NumericUpDownSubredditDataLimit.Value

        Dim postsList As JArray = Await apiHandler.AsyncGetSubredditPosts(subreddit:=subreddit, sort:=sort, limit:=limit)
        Dim isValid As Boolean = CoreUtils.IsValidData(data:=postsList)

        ' If the API data is valid, setup a DataGridView for the Posts.
        If isValid Then
            ' Iterate over each post and add its data to the DataGridView.
            For Each post As JObject In postsList
                AddToDataGridView(data:=post, form:=Posts, dataGridViewName:="DataGridViewPosts", RowKeys:=DataGridHelper.PostRowKeys)
            Next

            Posts.Text = $"Showing {sort} {limit} posts from r/{subreddit}"
            Posts.Show()

            ' Prompt to save data if the conditions are met.
            CoreUtils.PromptSaveData(data:=postsList, title:=$"Subreddit ({subreddit}) posts")
        End If
    End Function

    ''' <summary>
    ''' Asynchronously load a subreddit's data and update either the FormSubredditProfile or Posts.or forms 
    ''' depending on which Radio Button is checked.
    ''' </summary>
    ''' <param name="subreddit">The username to fetch data for.</param>
    Public Shared Async Function LoadSubredditDataAsync(subreddit As String) As Task
        If Main.RadioButtonSubredditProfile.Checked Then
            Await SubredditProfile(subreddit:=subreddit)
        ElseIf Main.RadioButtonSubredditPosts.Checked Then
            Await SubredditPosts(subreddit:=subreddit)
        End If
    End Function

    ''' <summary>
    ''' Asynchronously loads the posts of a specific user and updates the DataGridView on the given form.
    ''' </summary>
    ''' <param name="username">The Reddit username for which to fetch post data.</param>
    ''' <param name="form">The Posts.object that contains the DataGridView to be updated.</param>
    ''' <returns>A Task representing the asynchronous operation.</returns>
    Public Shared Async Function LoadSearchResultsAsync(query As String, form As Posts) As Task
        Dim sort As String = Main.ComboBoxSearchResultListing.Text
        Dim limit As Integer = Main.NumericUpDownSearchResultLimit.Value

        Dim Results As JArray = Await apiHandler.AsyncSearchPosts(query:=query, sort:=sort, limit:=limit)
        Dim isValid As Boolean = CoreUtils.IsValidData(data:=Results)

        ' If the API data is valid, setup a DataGridView for the Posts.
        If isValid Then
            ' Iterate over each post and add its data to the DataGridView.
            For Each Result As JObject In Results
                AddToDataGridView(data:=Result, form:=form, dataGridViewName:="DataGridViewPosts", RowKeys:=DataGridHelper.PostRowKeys)
            Next
            form.Text = $"Showing {limit} {sort} search results for `{query}`"
            form.Show()

            ' Prompt to save data if the conditions are met.
            CoreUtils.PromptSaveData(data:=Results, title:=$"Search results ")
        End If
    End Function

    ''' <summary>
    ''' Asynchronously loads the posts from the Reddit front page and updates the DataGridView in the Posts.form.
    ''' </summary>
    ''' <param name="form">The Posts.object that contains the DataGridView to be updated.</param>
    ''' <returns>A Task representing the asynchronous operation.</returns>
    Public Shared Async Function LoadFrontPagePostsAsync(form As Posts) As Task
        Dim sort As String = Main.ComboBoxFrontPageDataListing.Text
        Dim limit As Integer = Main.NumericUpDownFrontPageDataLimit.Value

        Dim Posts As JArray = Await apiHandler.AsyncGetFrontPagePosts(sort:=sort, limit:=limit)
        Dim isValid As Boolean = CoreUtils.IsValidData(data:=Posts)

        ' If the API data is valid, setup a DataGridView for the Posts.
        If isValid Then
            ' Iterate over each post and add its data to the DataGridView.
            For Each post As JObject In Posts
                AddToDataGridView(data:=post, form:=form, dataGridViewName:="DataGridViewPosts", RowKeys:=DataGridHelper.PostRowKeys)
            Next

            form.Text = $"{sort} {limit} posts from the front page"
            form.Show()

            ' Prompt to save data if the conditions are met.
            CoreUtils.PromptSaveData(data:=Posts, title:=$"Front page posts")
        End If
    End Function
End Class


''' <summary>
''' Class for data mapping and column headers in DataGridView.
''' </summary>
Public Class DataGridHelper

    ''' <summary>
    ''' Gets the default column headers for DataGridView with "Property" and "Value" columns.
    ''' </summary>
    Public Shared ReadOnly DefaultColumnHeaders As New Dictionary(Of String, String) From {
        {"Property", "Property"},
        {"Value", "Value"}
    }

    ''' <summary>
    ''' Dictionary that maps the JSON key to a user-friendly header name for user profiles.
    ''' </summary>
    Public Shared ReadOnly UserProfileHeaderMapping As New Dictionary(Of String, String) From {
        {"subreddit", "Name"},
        {"id", "ID"},
        {"hide_from_robots", "Is Robot Indexable?"},
        {"is_gold", "Is Gold?"},
        {"is_mod", "Is Mod?"},
        {"is_employee", "Is Employee?"},
        {"is_blocked", "Is Blocked?"},
        {"accept_followers", "Accepts Followers"},
        {"has_subscribed", "Has Subscribed?"},
        {"verified", "Verified"},
        {"has_verified_email", "Has Verified Email?"},
        {"pref_show_snoovatar", "Show Snoovatar"},
        {"snoovatar_img", "Snoovatar Image"},
        {"snoovatar_size", "Snoovatar Size"},
        {"link_karma", "Link Karma"},
        {"comment_karma", "Comment Karma"},
        {"awarder_karma", "Awarder Karma"},
        {"awardee_karma", "Awardee Karma"},
        {"total_karma", "Total Karma"}
    }

    ''' <summary>
    ''' Dictionary that maps the JSON key to a user-friendly header name for user subreddit.
    ''' </summary
    Public Shared ReadOnly UserSubredditHeaderMapping As New Dictionary(Of String, String) From {
        {"display_name_prefixed", "Name"},
        {"accept_followers", "Accept Followers"},
        {"allowed_media_in_comments", "Allow Media in Comments"},
        {"banner_img", "Banner Image"},
        {"default_set", "Default Set"},
        {"disable_contributor_requests", "Disable Contributor Requests"},
        {"free_form_reports", "Free-form Reports"},
        {"icon_color", "Icon Color"},
        {"icon_img", "Icon Image"},
        {"icon_size", "Icon Size"},
        {"is_default_banner", "Is Default Banner"},
        {"is_default_icon", "Is Default Icon"},
        {"key_color", "Key Color"},
        {"link_flair_enabled", "Link Flair Enabled"},
        {"link_flair_position", "Link Flair Position"},
        {"over_18", "Is NSFW"},
        {"previous_names", "Previous Names"},
        {"primary_color", "Primary Color"},
        {"public_description", "Description"},
        {"quarantine", "Quarantine"},
        {"restrict_commenting", "Restrict Commenting"},
        {"restrict_posting", "Restrict Posting"},
        {"show_media", "Show Media"},
        {"submit_link_label", "Submit Link Label"},
        {"submit_text_label", "Submit Text Label"},
        {"subreddit_type", "Subreddit Type"},
        {"subscribers", "Subscribers"},
        {"url", "URL"}
    }

    ''' <summary>
    ''' Dictionary that maps the JSON key to a user-friendly header name for subreddit profiles.
    ''' </summary
    Public Shared ReadOnly SubRedditProfileHeaderMapping As New Dictionary(Of String, String) From {
        {"title", "Title"},
        {"public_description", "Description"},
        {"submit_text", "Rules"},
        {"id", "ID"},
        {"subreddit_type", "Subreddit Type"},
        {"accounts_active", "Current Active Users"},
        {"subscribers", "Subscribers"},
        {"lang", "Language"},
        {"free_form_reports", "Free Form Reports"},
        {"wiki_enabled", "Wiki Enabled"},
        {"over18", "Over 18"},
        {"spoilers_enabled", "Spoilers"},
        {"icon_img", "Icon Image"},
        {"icon_size", "Icon Size"},
        {"submit_link_label", "Submit Link Label"},
        {"public_traffic", "Public Traffic"},
        {"whitelist_status", "Whitelist Status"},
        {"show_media", "Show Media"},
        {"submit_text_from_label", "Submit Text From Label"},
        {"user_sr_theme_enabled", "User Sr Theme Enabled"},
        {"show_media_preview", "Show Media Preview"},
        {"comment_score_hide_mins", "Comment Score Hide Mins"},
        {"allow_talks", "Allow Talks"},
        {"allow_polls", "Allow Polls"},
        {"allow_videogifs", "Allow Video Gifs"},
        {"allow_video_uploads", "Allow Video Uploads"},
        {"allow_videos", "Allow Videos"},
        {"allow_images", "Allow Images"},
        {"allow_galleries", "Allow Galleries"},
        {"allow_predictions", "Allow Predictions"},
        {"allow_predictions_tournament", "Allow Predictions Tournament"},
        {"allow_prediction_contributors", "Allow Predictions Contributors"},
        {"allow_chat_post_creation", "Allow Chat Posts"},
        {"allow_discovery", "Allow Discovery"},
        {"banner_img", "Banner Image"},
        {"is_default_banner", "Is Default Banner"},
        {"banner_background_color", "Banner Background Color"},
        {"banner_background_img", "Banner Background Image"},
        {"mobile_banner_img", "Mobile Banner Image"},
        {"header_size", "Header Size"},
        {"header_title", "Header Title"},
        {"header_img", "Header Image"},
        {"header_background_color", "Header Background Color"},
        {"header_background_img", "Header Background Image"},
        {"link_flair_enabled", "Link Flair Enabled"},
        {"can_assign_link_flair", "Can Assign Link Flair"},
        {"link_flair_position", "Link Flair Position"},
        {"user_flair_type", "User Flair Type"},
        {"user_flair_position", "User Flair Position"},
        {"user_flair_enabled_in_sr", "User Flair Enabled In Sr"},
        {"can_assign_user_flair", "Can Assign User Flair"},
        {"user_flair_richtext", "User Flair Richtext"}
    }

    ''' <summary>
    ''' List of keys for post rows in a DataGridView.
    ''' </summary>
    Public Shared ReadOnly PostRowKeys As New List(Of String) From {
        "author",
        "title",
        "selftext",
        "id",
        "subreddit_name_prefixed",
        "subreddit_type",
        "ups",
        "upvote_ratio",
        "downs",
        "thumbnail",
        "score",
        "num_comments",
        "total_awards_received",
        "domain",
        "permalink",
        "link_flair_text",
        "gilded",
        "over_18",
        "is_crosspostable",
        "edited",
        "is_robot_indexable",
        "stickied",
        "locked",
        "is_original_content",
        "is_reddit_media_domain",
        "archived",
        "quarantine",
        "created"
    }

    ''' <summary>
    ''' List of keys for comment rows in a DataGridView.
    ''' </summary>
    Public Shared ReadOnly CommentRowKeys As New List(Of String) From {
        "subreddit_name_prefixed",
        "subreddit_type",
        "author",
        "author_flair_type",
        "author_premium",
        "link_title",
        "body",
        "link_id",
        "link_permalink",
        "ups",
        "downs",
        "score",
        "score_hidden",
        "replies",
        "total_awards_received",
        "gilded",
        "over_18",
        "edited",
        "stickied",
        "locked",
        "archived",
        "quarantined",
        "created"
    }

End Class

