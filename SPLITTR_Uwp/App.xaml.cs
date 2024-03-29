﻿using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.Extensions.DependencyInjection;
using SPLITTR_Uwp.Dependencies;
using SPLITTR_Uwp.Services;

namespace SPLITTR_Uwp;

public sealed partial class App : Application
{
    private static IServiceProvider _container;

    public static IServiceProvider Container
    {
        get
        {
            return _container ??= GetServiceProvider();
        }

    }

        

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        InitializeComponent();
    }

    private static IServiceProvider GetServiceProvider()
    {
        return new ServiceCollection().AddDependencies().BuildServiceProvider();
    }

    public static bool IsAppTitleBarButtonVisible
    {
        get => SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility == AppViewBackButtonVisibility.Visible;
        set => SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = value ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
    }


    /// <summary>
    /// Invoked when the application is launched normally by the end user.  Other entry points
    /// will be used such as when the application is launched to open a specific file.
    /// </summary>
    /// <param name="e">Details about the launch request and process.</param>
    protected  override void OnLaunched(LaunchActivatedEventArgs e)
    {
        Frame rootFrame = Window.Current.Content as Frame;

        // Do not repeat app initialization when the Window already has content,
        // just ensure that the window is active
        if (rootFrame == null)
        {
            // Create a Frame to act as the navigation context and navigate to the first page
            rootFrame = new Frame();

            rootFrame.NavigationFailed += OnNavigationFailed;

            if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
            {
                //TODO: Load state from previously suspended application
            }

            // Place the frame in the current Window
            Window.Current.Content = rootFrame;


        }
        //Initializing The DI Container
        _container =   GetServiceProvider();

        if (e.PrelaunchActivated == false)
        {
            if (rootFrame.Content == null)
            {
                var stateManager = Container.GetService(typeof(IStateService)) as IStateService;
                stateManager?.VerifySessionIfAvailable();
            }
            // Ensure the current window is active
            Window.Current.Activate();
        }
            
        //Registering Root Frame for Theme Change Functionality
        ThemeHelperService.RegisterElement(rootFrame);

        //Setting Initial preset  Theme
        rootFrame.RequestedTheme = ThemeHelperService.GetPreferenceThemeIfSet();
            
        //Setting Application title bar to Required theme color
        AccentColorService.Register(Window.Current.CoreWindow);


    }

    /// <summary>
    /// Invoked when Navigation to a certain page fails
    /// </summary>
    /// <param name="sender">The Frame which failed navigation</param>
    /// <param name="e">Details about the navigation failure</param>
    private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
    }

    /// <summary>
    /// Invoked when application execution is being suspended.  Application state is saved
    /// without knowing whether the application will be terminated or resumed with the contents
    /// of memory still intact.
    /// </summary>
    /// <param name="sender">The source of the suspend request.</param>
    /// <param name="e">Details about the suspend request.</param>
    private void OnSuspending(object sender, SuspendingEventArgs e)
    {
        var deferral = e.SuspendingOperation.GetDeferral();
        //TODO: Save application state and stop any background activity
        deferral.Complete();
    }

}