from .general import console

__all__ = ["Colour", "Notify"]


class Colour:
    """
    Provides rich formatting colour codes for styling text.

    Attributes:
        blue (str): Rich code for blue text.
        bold (str): Rich code for bold text.
        cyan (str): Rich code for cyan text.
        green (str): Rich code for green text.
        italic (str): Rich code for italic text.
        yellow (str): Rich code for yellow text.
        purple (str): Rich code for purple text.
        red (str): Rich code for red text.
        white (str): Rich code for white text.
        reset (str): Rich code to reset text formatting.
    """

    blue: str = "[blue]"
    bold: str = "[bold]"
    cyan: str = "[cyan]"
    green: str = "[green]"
    italic: str = "[italic]"
    yellow: str = "[yellow]"
    purple: str = "[purple]"
    red: str = "[red]"
    white: str = "[white]"
    reset: str = "[/]"


class Notify:
    """
    Provides static methods for printing and/or logging formatted notifications to the console.
    """

    @staticmethod
    def info(message: str):
        """
        Prints an `informational` message to the console.

        :param message: The message to be displayed.
        :type message: str
        """
        console.print(f"[{Colour.green}*{Colour.reset}] {message}")

    @staticmethod
    def warning(message: str):
        """
        Prints a `warning` message to the console.

        :param message: The message to be displayed.
        :type message: str
        """
        console.print(f"[{Colour.yellow}!{Colour.reset}] {message}")

    @staticmethod
    def exception(error: Exception, **kwargs: str):
        """
        Logs an exception message to the console.

        :param error: The exception that triggered the error.
        :type error: Exception
        :param kwargs: Additional keyword arguments for optional context.
            - error_type (str, optional): The type of error (e.g., `unexpected`, `HTTP`, `API`).
            - error_context (str, optional): The context where the error occurred (e.g., `while updating database`).
        """
        exception_type: str = kwargs.get("exception_type", "")
        exception_context: str = kwargs.get("exception_context", "")

        # Combine the error type and location into a single formatted string
        formatted_exception: str = (
            f"An {Colour.bold}{exception_type}{Colour.reset} error occurred"
            if exception_type
            else "An error occurred"
        )
        if exception_context:
            formatted_exception += (
                f" ({Colour.italic}{exception_context}{Colour.reset})"
            )

        console.log(f"[{Colour.red}✘{Colour.reset}] {formatted_exception}: {error}")

    @staticmethod
    def ok(message: str):
        """
        Prints a `success` message (with a checkmark).

        :param message: The message to be displayed.
        :type message: str
        """
        console.print(f"[{Colour.green}✔{Colour.reset}] {message}")

    @staticmethod
    def raise_exception(base_exception: type[BaseException], message: str):
        """
        Raises a specified base `exception` with the provided message.

        :param base_exception: Base exception class to be raised.
        :type base_exception: type[Exception]
        :param message: The exception message to be displayed.
        :type message: str
        :raises Exception: The specified base exception with the provided message.
        """
        raise base_exception(message)

# -------------------------------- END ----------------------------------------- #