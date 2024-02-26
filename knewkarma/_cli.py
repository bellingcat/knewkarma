# +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ #

import argparse
import asyncio
import os
from datetime import datetime
from typing import get_args

import aiohttp
from rich.markdown import Markdown
from rich.tree import Tree
from rich_argparse import RichHelpFormatter

from . import RedditUser, RedditCommunity, RedditPosts
from ._api import get_updates
from ._coreutils import (
    console,
    pathfinder,
    export_dataframe,
    filename_timestamp,
    create_dataframe,
    show_exported_files,
    systeminfo,
)
from .base import RedditSearch, RedditCommunities, RedditPost
from .docs import (
    PROGRAM_DIRECTORY,
    DESCRIPTION,
    COPYRIGHT,
    DATA_TIMEFRAME,
    DATA_SORT_CRITERION,
    Version,
    OPERATIONS_TEXT,
    COMMUNITY_EXAMPLES,
    COMMUNITIES_EXAMPLES,
    POST_EXAMPLES,
    POSTS_EXAMPLES,
    POSTS_LISTINGS,
    SEARCH_EXAMPLES,
    USER_EXAMPLES,
    LICENSE,
)


# +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ #


def create_arg_parser() -> argparse.ArgumentParser:
    """
    Creates and configures an argument parser for the command line arguments.

    :return: A configured argparse.ArgumentParser object ready to parse the command line arguments.
    :rtype: argparse.ArgumentParser
    """
    # ------------------------------------------------------------------------------- #

    main_parser = argparse.ArgumentParser(
        description=Markdown(DESCRIPTION, style="argparse.text"),
        epilog=Markdown(LICENSE, style="argparse.text"),
        formatter_class=RichHelpFormatter,
    )
    subparsers = main_parser.add_subparsers(dest="mode", help="operation mode")
    main_parser.add_argument(
        "-t",
        "--timeframe",
        type=str,
        default="all",
        choices=list(get_args(DATA_TIMEFRAME)),
        help="timeframe to get [[bold][green]bulk[/][/]] data from (default: %(default)s)",
    )
    main_parser.add_argument(
        "-s",
        "--sort",
        type=str,
        default="all",
        choices=list(get_args(DATA_SORT_CRITERION)),
        help="[[bold][green]bulk[/][/]] sort criterion (default: %(default)s)",
    )
    main_parser.add_argument(
        "-l",
        "--limit",
        type=int,
        default=100,
        metavar="NUMBER",
        help="[[bold][green]bulk[/][/]] data output limit (default: %(default)s)",
    )
    main_parser.add_argument(
        "-sleep",
        type=int,
        default=20,
        metavar="SECONDS",
        help="[[bold][green]bulk data[/][/]] sleep delay (seconds) after each request (default: %(default)s)",
    )
    main_parser.add_argument(
        "-e",
        "--export",
        type=str,
        metavar="FILETYPES",
        help="a comma-separated list of file types to export the output to (supported: [green]csv,html,json,xml[/])",
    )
    main_parser.add_argument(
        "-u",
        "--updates",
        help="check for updates on run",
        action="store_true",
    )
    main_parser.add_argument(
        "-v",
        "--version",
        version=Markdown(f"Knew Karma {Version.release} {COPYRIGHT}"),
        action="version",
    )

    # ------------------------------------------------------------------------------- #

    community_parser = subparsers.add_parser(
        "community",
        help="community (subreddit) operations",
        description=Markdown(
            OPERATIONS_TEXT.format("Community (Subreddit)"),
            style="argparse.text",
        ),
        epilog=Markdown(COMMUNITY_EXAMPLES),
        formatter_class=RichHelpFormatter,
    )
    community_parser.add_argument(
        "community",
        help="community name",
    )
    community_parser.add_argument(
        "-p",
        "--profile",
        help="get a community's profile",
        action="store_true",
    )
    community_parser.add_argument(
        "-s",
        "--search",
        metavar="KEYWORD",
        help="get a community's posts that contain the specified keyword",
        type=str,
    )
    community_parser.add_argument(
        "-pp",
        "--posts",
        help="get a community's posts",
        action="store_true",
    )
    community_parser.add_argument(
        "-wp",
        "--wiki-page",
        dest="wiki_page",
        help="get a community's specified wiki page data",
        metavar="WIKI_PAGE",
    )
    community_parser.add_argument(
        "-wps",
        "--wiki-pages",
        dest="wiki_pages",
        help="get a community's wiki pages",
        action="store_true",
    )

    # ------------------------------------------------------------------------------- #

    communities_parser = subparsers.add_parser(
        "communities",
        help="communities (subreddits) operations",
        description=Markdown(
            OPERATIONS_TEXT.format("Communities (Subreddits)"),
            style="argparse.text",
        ),
        epilog=Markdown(COMMUNITIES_EXAMPLES),
        formatter_class=RichHelpFormatter,
    )
    communities_parser.add_argument(
        "-a",
        "--all",
        help="get all communities",
        action="store_true",
    )
    communities_parser.add_argument(
        "-d",
        "--default",
        help="get default communities",
        action="store_true",
    )
    communities_parser.add_argument(
        "-n",
        "--new",
        help="get new communities",
        action="store_true",
    )
    communities_parser.add_argument(
        "-p",
        "--popular",
        help="get popular communities",
        action="store_true",
    )

    # ------------------------------------------------------------------------------- #

    post_parser = subparsers.add_parser(
        "post",
        help="post operations",
        description=Markdown(OPERATIONS_TEXT.format("Post"), style="argparse.text"),
        epilog=Markdown(POST_EXAMPLES),
        formatter_class=RichHelpFormatter,
    )

    post_parser.add_argument("id", help="post id", type=str)
    post_parser.add_argument("community", help="post source community", type=str)
    post_parser.add_argument(
        "-p", "--profile", help="get post 'profile' data", action="store_true"
    )
    post_parser.add_argument(
        "-c", "--comments", help="get post comments", action="store_true"
    )

    # ------------------------------------------------------------------------------- #

    posts_parser = subparsers.add_parser(
        "posts",
        help="posts operations",
        description=Markdown(OPERATIONS_TEXT.format("Posts"), style="argparse.text"),
        epilog=Markdown(POSTS_EXAMPLES),
        formatter_class=RichHelpFormatter,
    )
    posts_parser.add_argument(
        "-n",
        "--new",
        help="get new posts",
        action="store_true",
    )
    posts_parser.add_argument(
        "-f",
        "--front-page",
        help="get posts from the reddit front-page",
        action="store_true",
    )
    posts_parser.add_argument(
        "-l",
        "--listing",
        default="all",
        help="get posts from a specified listing",
        choices=list(get_args(POSTS_LISTINGS)),
    )

    # ------------------------------------------------------------------------------- #

    search_parser = subparsers.add_parser(
        "search",
        help="search operations",
        description=Markdown(OPERATIONS_TEXT.format("Search"), style="argparse.text"),
        epilog=Markdown(SEARCH_EXAMPLES),
        formatter_class=RichHelpFormatter,
    )
    search_parser.add_argument("query", help="search query")
    search_parser.add_argument(
        "-u", "--users", help="search users", action="store_true"
    )
    search_parser.add_argument(
        "-p", "--posts", help="search posts", action="store_true"
    )
    search_parser.add_argument(
        "-c", "--communities", help="search communities", action="store_true"
    )

    # ------------------------------------------------------------------------------- #

    user_parser = subparsers.add_parser(
        "user",
        help="user operations",
        description=Markdown(OPERATIONS_TEXT.format("User"), style="argparse.text"),
        epilog=Markdown(USER_EXAMPLES),
        formatter_class=RichHelpFormatter,
    )
    user_parser.add_argument("username", help="username")
    user_parser.add_argument(
        "-p",
        "--profile",
        help="get a user's profile",
        action="store_true",
    )
    user_parser.add_argument(
        "-c",
        "--comments",
        help="get a user's comments",
        action="store_true",
    )

    user_parser.add_argument(
        "-o",
        "--overview",
        help="get a user's most recent comment activity",
        action="store_true",
    )
    user_parser.add_argument(
        "-pp",
        "--posts",
        action="store_true",
        help="get a user's posts",
    )
    user_parser.add_argument(
        "-sp",
        "--search-posts",
        dest="search_posts",
        metavar="KEYWORD",
        help="get a user's posts that contain the specified keyword",
        type=str,
    )
    user_parser.add_argument(
        "-sc",
        "--search-comments",
        dest="search_comments",
        metavar="KEYWORD",
        help="get a user's comments that contain the specified keyword",
        type=str,
    )
    user_parser.add_argument(
        "-mc",
        "--moderated-communities",
        dest="moderated_communities",
        help="get communities moderated by the user",
        action="store_true",
    )
    user_parser.add_argument(
        "-tc",
        "--top-communities",
        dest="top_communities",
        metavar="TOP_N",
        type=int,
        help="get a user's top n communities based on community frequency in n posts",
    )

    return main_parser


# +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ #


async def call_functions(args: argparse.Namespace, function_mapping: dict):
    """
    Calls command-line arguments' functions based on user-input.

    :param args: Argparse namespace object  containing parsed command-line arguments.
    :type args: argparse.Namespace
    :param function_mapping: Mapping of command-line commands to their respective functions
    :type function_mapping: dict
    """

    async with aiohttp.ClientSession() as request_session:
        if args.updates:
            await get_updates(session=request_session)

        mode_action = function_mapping.get(args.mode)
        directory: str = ""
        for action, function in mode_action:
            arg_is_present: bool = False
            if getattr(args, action, False):
                arg_is_present = True
                # ------------------------------------------------------------ #

                if args.export:
                    # Create path to main directory in which target data files will be exported
                    directory = os.path.join(PROGRAM_DIRECTORY, args.mode, action)

                    # Create file directories for supported data file types
                    pathfinder(
                        directories=[
                            os.path.join(directory, "csv"),
                            os.path.join(directory, "html"),
                            os.path.join(directory, "json"),
                            os.path.join(directory, "xml"),
                        ]
                    )

                # ----------------------------------------------------------- #

                function_data = await function(session=request_session)
                if function_data:
                    dataframe = create_dataframe(data=function_data)

                    # Print the DataFrame, excluding the 'raw_data' column if it exists
                    console.log(dataframe)

                    # ------------------------------------------------------- #

                    if args.export:
                        export_dataframe(
                            dataframe=dataframe,
                            filename=filename_timestamp(),
                            directory=directory,
                            formats=args.export.split(","),
                        )

                        # Show exported files
                        tree = Tree(
                            f":open_file_folder: [bold]{directory}[/]",
                            guide_style="bold bright_blue",
                        )
                        show_exported_files(tree=tree, directory=directory)
                        console.log(tree)

                    # -------------------------------------------------------- #

                break

        if not arg_is_present:
            console.log(
                f"knewkarma {args.mode}: missing one or more expected argument(s)"
            )


# +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ #


def stage_and_start():
    """
    Main entrypoint for the Knew Karma command-line interface.
    """
    # ------------------------------------------------------------------------------- #

    parser = create_arg_parser()
    args: argparse = parser.parse_args()

    start_time: datetime = datetime.now()

    # ------------------------------------------------------------------------------- #

    limit: int = args.limit
    sort = args.sort
    timeframe = args.timeframe

    search_query = args.query if hasattr(args, "query") else None

    # ------------------------------------------------------------------------------- #

    user = RedditUser(
        username=args.username if hasattr(args, "username") else None,
    )
    search = RedditSearch()
    community = RedditCommunity(
        community=args.community if hasattr(args, "community") else None,
    )
    communities = RedditCommunities()
    post = RedditPost(
        id=args.id if hasattr(args, "id") else None,
        community=args.community if hasattr(args, "community") else None,
    )
    posts = RedditPosts()

    # ------------------------------------------------------------------------------- #

    function_mapping: dict = {
        "user": [
            ("profile", lambda session: user.profile(session=session)),
            (
                "posts",
                lambda session: user.posts(
                    limit=limit, sort=sort, timeframe=timeframe, session=session
                ),
            ),
            (
                "comments",
                lambda session: user.comments(
                    limit=limit, sort=sort, timeframe=timeframe, session=session
                ),
            ),
            ("overview", lambda session: user.overview(limit=limit, session=session)),
            (
                "moderated_communities",
                lambda session: user.moderated_communities(session=session),
            ),
            (
                "search_posts",
                lambda session: user.search_posts(
                    keyword=args.search_posts,
                    limit=limit,
                    sort=sort,
                    timeframe=timeframe,
                    session=session,
                ),
            ),
            (
                "search_comments",
                lambda session: user.search_comments(
                    keyword=args.search_comments,
                    limit=limit,
                    sort=sort,
                    timeframe=timeframe,
                    session=session,
                ),
            ),
            (
                "top_communities",
                lambda session: user.top_communities(
                    top_n=args.top_communities
                    if hasattr(args, "top_communities")
                    else None,
                    limit=limit,
                    sort=sort,
                    timeframe=timeframe,
                    session=session,
                ),
            ),
        ],
        "community": [
            ("profile", lambda session: community.profile(session=session)),
            (
                "posts",
                lambda session: community.posts(
                    limit=limit, sort=sort, timeframe=timeframe, session=session
                ),
            ),
            (
                "search",
                lambda session: community.search(
                    keyword=args.search,
                    limit=limit,
                    sort=sort,
                    timeframe=timeframe,
                    session=session,
                ),
            ),
            ("wiki_pages", lambda session: community.wiki_pages(session=session)),
            (
                "wiki_page",
                lambda session: community.wiki_page(
                    page=args.wiki_page if hasattr(args, "wiki_page") else None,
                    session=session,
                ),
            ),
        ],
        "communities": [
            ("all", lambda session: communities.all(limit=limit, session=session)),
            (
                "default",
                lambda session: communities.default(limit=limit, session=session),
            ),
            ("new", lambda session: communities.new(limit=limit, session=session)),
            (
                "popular",
                lambda session: communities.popular(limit=limit, session=session),
            ),
        ],
        "post": [
            ("profile", lambda session: post.profile(session=session)),
            (
                "comments",
                lambda session: post.comments(
                    limit=limit,
                    sort=sort,
                    session=session,
                ),
            ),
        ],
        "posts": [
            ("new", lambda session: posts.new(limit=limit, sort=sort, session=session)),
            (
                "front_page",
                lambda session: posts.front_page(
                    limit=limit, sort=sort, timeframe=timeframe, session=session
                ),
            ),
            (
                "listing",
                lambda session: posts.listing(
                    listings_name=args.listing,
                    limit=limit,
                    sort=sort,
                    timeframe=timeframe,
                    session=session,
                ),
            ),
        ],
        "search": [
            (
                "users",
                lambda session: search.users(
                    query=search_query, limit=limit, session=session
                ),
            ),
            (
                "communities",
                lambda session: search.communities(
                    query=search_query, limit=limit, session=session
                ),
            ),
            (
                "posts",
                lambda session: search.posts(
                    query=search_query,
                    limit=limit,
                    sort=sort,
                    timeframe=timeframe,
                    session=session,
                ),
            ),
        ],
    }

    # ------------------------------------------------------------------------------- #

    if args.mode:
        print(
            """
┓┏┓         ┓┏┓
┃┫ ┏┓┏┓┓┏┏  ┃┫ ┏┓┏┓┏┳┓┏┓
┛┗┛┛┗┗ ┗┻┛  ┛┗┛┗┻┛ ┛┗┗┗┻"""
        )
        with console.status(
            status=f"Working: [bold]Knew Karma (CLI) [cyan]{Version.release}[/][/]",
            spinner="dots2",
        ):
            # ------------------------------------- #
            for key, value in systeminfo().items():
                console.log(f"◉ [bold]{key}[/]: {value}")

            print(f"{'='*40}")
            # ------------------------------------- #

            try:
                start_time: datetime = datetime.now()
                asyncio.run(
                    call_functions(args=args, function_mapping=function_mapping)
                )
            except KeyboardInterrupt:
                console.log(
                    "[yellow]✔[/] User interruption detected ([yellow]Ctrl+C[/])"
                )
            finally:
                elapsed_time = datetime.now() - start_time
                console.log(
                    f"[green]✔[/] Done! {elapsed_time.total_seconds():.2f} seconds elapsed."
                )
    else:
        parser.print_usage()


# +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ #
