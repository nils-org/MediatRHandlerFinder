using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using EnvDTE;
using EnvDTE80;
using Task = System.Threading.Tasks.Task;

namespace MediatRvs
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(MediatrToolWindow))]
    [Guid(PackageGuids.guidShowHandlersToolWindowPackageString)]
    public sealed class MediatRvsPackage : AsyncPackage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MediatRvsPackage"/> class.
        /// </summary>
        public MediatRvsPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            
            Logger.Initialize(this, Vsix.Name);
            await MediatrToolWindowCommand.InitializeAsync(this).ConfigureAwait(true);

            // do stuff with DTE
            if (!(await GetServiceAsync(typeof(DTE)).ConfigureAwait(true) is DTE2 dte))
            {
                return;
            }

            dte.Events.DTEEvents.OnStartupComplete += () =>
            {
                if (dte.Solution != null)
                {
                    // loaded with solution.
                    // refresh ToolWindow?
                }
            };

            dte.Events.SolutionEvents.Opened += () =>
            {
                // a new solution opened.
                // refresh ToolWindow?
            };
        }

        public override IVsAsyncToolWindowFactory GetAsyncToolWindowFactory(Guid toolWindowType)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (toolWindowType == typeof(MediatrToolWindow).GUID)
            {
                return this;
            }

            return base.GetAsyncToolWindowFactory(toolWindowType);
        }

        protected override string GetToolWindowTitle(Type toolWindowType, int id)
        {
            if (toolWindowType == typeof(MediatrToolWindow))
            {
                return $"{Names.ToolWindow.Title} loading";
            }

            return base.GetToolWindowTitle(toolWindowType, id);
        }

        #endregion
    }
}
