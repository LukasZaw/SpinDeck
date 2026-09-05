using SpinDeck_Win_app.Models;
using System.Diagnostics;
using System.IO;

namespace SpinDeck_Win_app.Services
{
    public sealed class ActionExecutionResult
    {
        private ActionExecutionResult(bool succeeded, string? errorMessage)
        {
            Succeeded = succeeded;
            ErrorMessage = errorMessage;
        }

        public bool Succeeded { get; }

        public string? ErrorMessage { get; }

        public static ActionExecutionResult Success()
        {
            return new ActionExecutionResult(true, null);
        }

        public static ActionExecutionResult Failure(string message)
        {
            return new ActionExecutionResult(false, message);
        }
    }

    public sealed class ActionExecutor
    {
        public ActionExecutionResult Execute(ActionConfiguration? action)
        {
            if (action == null)
            {
                return ActionExecutionResult.Failure(
                    "No action is selected.");
            }

            if (string.IsNullOrWhiteSpace(action.Value))
            {
                return ActionExecutionResult.Failure(
                    "The action has no configured value.");
            }

            return action.Type switch
            {
                ActionType.Browser => ExecuteBrowser(action.Value),
                ActionType.Application => ExecuteApplication(action.Value),
                _ => ActionExecutionResult.Failure(
                    "This action type is not supported.")
            };
        }

        private static ActionExecutionResult ExecuteBrowser(string value)
        {
            if (!Uri.TryCreate(
                    value.Trim(),
                    UriKind.Absolute,
                    out Uri? uri) ||
                (uri.Scheme != Uri.UriSchemeHttp &&
                 uri.Scheme != Uri.UriSchemeHttps))
            {
                return ActionExecutionResult.Failure(
                    "The address must use the http or https protocol.");
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = uri.ToString(),
                    UseShellExecute = true
                });

                return ActionExecutionResult.Success();
            }
            catch (UnauthorizedAccessException)
            {
                return ActionExecutionResult.Failure(
                    "Permission was denied when opening the address in the browser.");
            }
            catch (InvalidOperationException)
            {
                return ActionExecutionResult.Failure(
                    "The default browser could not be opened.");
            }
            catch (System.ComponentModel.Win32Exception)
            {
                return ActionExecutionResult.Failure(
                    "The default browser could not be opened.");
            }
        }

        private static ActionExecutionResult ExecuteApplication(string value)
        {
            string path = value.Trim();

            if (!Path.IsPathFullyQualified(path))
            {
                return ActionExecutionResult.Failure(
                    "The application path must be a full path to an .exe file.");
            }

            if (!string.Equals(
                    Path.GetExtension(path),
                    ".exe",
                    StringComparison.OrdinalIgnoreCase))
            {
                return ActionExecutionResult.Failure(
                    "The application action must point to an .exe file.");
            }

            if (!File.Exists(path))
            {
                return ActionExecutionResult.Failure(
                    "The selected application file does not exist.");
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = false
                });

                return ActionExecutionResult.Success();
            }
            catch (UnauthorizedAccessException)
            {
                return ActionExecutionResult.Failure(
                    "Permission was denied when starting the selected application.");
            }
            catch (System.ComponentModel.Win32Exception)
            {
                return ActionExecutionResult.Failure(
                    "Windows could not start the selected application.");
            }
            catch (InvalidOperationException)
            {
                return ActionExecutionResult.Failure(
                    "The selected application could not be started.");
            }
        }
    }
}