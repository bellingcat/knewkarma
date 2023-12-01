import asyncio

import aiohttp

from knewkarma import RedditUser


# Define an asynchronous function to fetch User
async def async_user(username: str, data_limit: int, data_sort: str):
    # Initialize a RedditUser object with the specified username, data limit, and sorting criteria
    user = RedditUser(username=username, data_limit=data_limit, data_sort=data_sort)

    # Establish an asynchronous HTTP session
    async with aiohttp.ClientSession() as session:
        # Fetch user's profile
        profile = await user.profile(session=session)

        # Fetch user's posts
        posts = await user.posts(session=session)

        # Fetch user's comments
        comments = await user.comments(session=session)

        print(profile)
        print(posts)
        print(comments)


# Run the asynchronous function with a specified username, data limit, and sorting parameter
asyncio.run(async_user(username="automoderator", data_limit=100, data_sort="all"))