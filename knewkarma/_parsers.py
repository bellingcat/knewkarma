# +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ #

from typing import Union

from ._coreutils import timestamp_to_locale


# +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ #


def parse_users(data: Union[list[dict], dict]) -> Union[list[dict], dict]:
    def build_user(user: dict) -> dict:
        return {
            "name": user.get("name"),
            "id": user.get("id"),
            "avatar_url": user.get("icon_img"),
            "is_verified": user.get("verified"),
            "has_verified_email": user.get("has_verified_email"),
            "is_gold": user.get("is_gold"),
            "is_mod": user.get("is_mod"),
            "is_blocked": user.get("is_blocked"),
            "is_employee": user.get("is_employee"),
            "hidden_from_bots": user.get("hide_from_robots"),
            "accepts_followers": user.get("accept_followers"),
            "comment_karma": user.get("comment_karma"),
            "link_karma": user.get("link_karma"),
            "awardee_karma": user.get("awardee_karma"),
            "total_karma": user.get("total_karma"),
            "community": user.get("subreddit"),
            "created": timestamp_to_locale(timestamp=user.get("created"))
            if user.get("created")
            else "NaN",
        }

    # ------------------------------------------------------------------------------- #

    converted_data = None
    if isinstance(data, list) and len(data) != 0:
        converted_data = [build_user(user.get("data")) for user in data]

    elif isinstance(data, dict) and "is_employee" in data:
        converted_data = build_user(data)

    return converted_data


# +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ #


def parse_posts(data: Union[dict, list]) -> Union[list[dict], dict]:
    def build_post(post: dict) -> dict:
        return {
            "author": post.get("author"),
            "title": post.get("title"),
            "body": post.get("selftext"),
            "id": post.get("id"),
            "community": post.get("subreddit"),
            "community_id": post.get("subreddit_id"),
            "community_type": post.get("subreddit_type"),
            "upvotes": post.get("ups"),
            "upvote_ratio": post.get("upvote_ratio"),
            "downvotes": post.get("downs"),
            "thumbnail": post.get("thumbnail"),
            "gilded": post.get("gilded"),
            "is_nsfw": post.get("over_18"),
            "is_shareable": post.get("is_reddit_media_domain"),
            "hide_from_bots": post.get("is_robot_indexable"),
            "permalink": post.get("permalink"),
            "is_locked": post.get("locked"),
            "is_archived": post.get("archived"),
            "domain": post.get("domain"),
            "score": post.get("score"),
            "edited": timestamp_to_locale(timestamp=post.get("edited"))
            if post.get("edited")
            else False,
            "comments": post.get("num_comments"),
            "created": timestamp_to_locale(timestamp=post.get("created"))
            if post.get("created")
            else "NaN",
        }

    if isinstance(data, dict):
        return build_post(post=data)

    elif isinstance(data, list):
        return [build_post(post.get("data")) for post in data if post.get("data")]


# +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ #


def parse_comments(comments: list[dict]) -> list[dict]:
    if len(comments) != 0:
        comments_list: list = []
        for comment in comments:
            comment_data: dict = comment.get("data")
            comments_list.append(
                {
                    "body": comment_data.get("body"),
                    "id": comment_data.get("id"),
                    "author": comment_data.get("author"),
                    "author_is_premium": comment_data.get("author_premium"),
                    "upvotes": comment_data.get("ups"),
                    "downvotes": comment_data.get("downs"),
                    "community": comment_data.get("subreddit_name_prefixed"),
                    "community_type": comment_data.get("subreddit_type"),
                    "post_id": comment_data.get("link_id"),
                    "post_title": comment_data.get("link_title"),
                    "is_nsfw": comment_data.get("over_18"),
                    "is_edited": comment_data.get("edited"),
                    "score": comment_data.get("score"),
                    "hidden_score": comment_data.get("score_hidden"),
                    "gilded": comment_data.get("gilded"),
                    "is_stickied": comment_data.get("stickied"),
                    "is_locked": comment_data.get("locked"),
                    "is_archived": comment_data.get("archived"),
                    "created": timestamp_to_locale(timestamp=comment.get("created"))
                    if comment.get("created")
                    else "NaN",
                }
            )

        return comments_list


# +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ #


def parse_communities(
    data: Union[list, dict], is_preview: bool = False
) -> Union[list[dict], dict]:
    def build_community(
        community: dict,
    ) -> dict:
        if is_preview:
            community_obj = {
                "name": community.get("display_name"),
                "id": community.get("id"),
                "type": community.get("subreddit_type"),
                "icon": community.get("community_icon", "").split("?")[0],
                "subscribers": community.get("subscribers"),
                "whitelist_status": community.get("whitelist_status"),
                "url": community.get("url"),
                "created": timestamp_to_locale(timestamp=community.get("created")),
            }
        else:
            community_obj = {
                "name": community.get("display_name"),
                "id": community.get("id"),
                "description": community.get("public_description"),
                "submit_text": community.get("submit_text"),
                "icon": community.get("community_icon", "").split("?")[0],
                "type": community.get("subreddit_type"),
                "subscribers": community.get("subscribers"),
                "current_active_users": community.get("accounts_active"),
                "is_nsfw": community.get("over18"),
                "language": community.get("lang"),
                "whitelist_status": community.get("whitelist_status"),
                "url": community.get("url"),
                "created": timestamp_to_locale(timestamp=community.get("created"))
                if community.get("created")
                else "NaN",
            }

        return community_obj

    # ------------------------------------------------------------------------------- #

    community_data = None
    if isinstance(data, list) and len(data) != 0:
        community_data = [
            build_community(community=community.get("data")) for community in data
        ]

    elif isinstance(data, dict) and "subreddit_type" in data:
        community_data = build_community(community=data)

    return community_data


# +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ #


def parse_community_wiki_page(wiki_page: dict) -> dict:
    if "revision_id" in wiki_page:
        page_data: dict = wiki_page.get("data")
        user: dict = page_data.get("revision_by").get("data")

        return {
            "revision_id": page_data.get("revision_id"),
            "revision_date": timestamp_to_locale(
                timestamp=page_data.get("revision_date")
            ),
            "content_markdown": page_data.get("content_md"),
            "revised_by": {
                "name": user.get("name"),
                "id": user.get("id"),
                "avatar_url": user.get("icon_img"),
                "is_verified": user.get("verified"),
                "has_verified_email": user.get("has_verified_email"),
                "is_gold": user.get("is_gold"),
                "is_mod": user.get("is_mod"),
                "is_blocked": user.get("is_blocked"),
                "is_employee": user.get("is_employee"),
                "hidden_from_bots": user.get("hide_from_robots"),
                "accepts_followers": user.get("accept_followers"),
                "comment_karma": user.get("comment_karma"),
                "link_karma": user.get("link_karma"),
                "awardee_karma": user.get("awardee_karma"),
                "total_karma": user.get("total_karma"),
                "community": user.get("subreddit"),
                "created": timestamp_to_locale(timestamp=user.get("created")),
            },
        }


# +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ #
