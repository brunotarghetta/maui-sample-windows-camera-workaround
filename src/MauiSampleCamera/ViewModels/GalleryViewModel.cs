﻿using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiSampleCamera.Models;

namespace MauiSampleCamera.ViewModels;

public partial class GalleryViewModel : ObservableObject
{
	// the captured files will be stored inside this folder
	private static readonly string GalleryFolder = FileSystem.AppDataDirectory;

	private readonly IMediaPicker _mediaPicker;

	public GalleryViewModel(IMediaPicker mediaPicker)
	{
		_mediaPicker = mediaPicker;

		// load the existing files on startup
		Files.Clear();
		Directory.GetFiles(GalleryFolder).ToList().ForEach(f => Files.Add(new CaptureDetails() { FileName = Path.GetFileName(f), Path = f }));

	}

	[ObservableProperty]
	private bool _isBusy;

	// contains references to the captured photos and videos
	public ObservableCollection<CaptureDetails> Files { get; } = new();

	[RelayCommand]
	private Task CapturePhoto()
	{
		return Capture(false);
	}

	[RelayCommand]
	private Task CaptureVideo()
	{
		return Capture(true);
	}

	private async Task Capture(bool isVideo)
	{
		// return if the app is already performing another operation
		if (IsBusy)
		{
			return;
		}

		// show an error message if the camera is not available on the device
		if (!_mediaPicker.IsCaptureSupported)
		{
			await Shell.Current.DisplayAlert("Error", "Camera not available", "OK");
			return;
		}

		try
		{
			// mark as busy while using the camera
			IsBusy = true;

			// open the camera and capture a photo or video
			FileResult file = isVideo ? await _mediaPicker.CaptureVideoAsync() : await _mediaPicker.CapturePhotoAsync();

			// file is null if the user cancels the operation
			if (file != null)
			{
				string localFilePath = Path.Combine(GalleryFolder, file.FileName);

				// save the file into the gallery folder
				using FileStream localFileStream = File.OpenWrite(localFilePath);
#if WINDOWS
				// on Windows file.OpenReadAsync() throws an exception
				using Stream sourceStream = File.OpenRead(file.FullPath);
#else
				using Stream sourceStream = await file.OpenReadAsync();
#endif

				await sourceStream.CopyToAsync(localFileStream);

				// add the file path to the list to display the picture on the main page
				Files.Add(new CaptureDetails()
				{
					FileName = file.FileName,
					Path = localFilePath,
				});
			}
		}
		catch (Exception ex)
		{
			await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
		}
		finally
		{
			IsBusy = false;
		}
	}

	[RelayCommand]
	private Task AddPhoto()
	{
		return AddToGallery(false);
	}

	[RelayCommand]
	private Task AddVideo()
	{
		return AddToGallery(true);
	}

	[RelayCommand]
	private Task OpenPhoto(CaptureDetails capture)
	{
		return Open(capture);
	}

	private async Task Open(CaptureDetails capture)
	{
		if (!_mediaPicker.IsCaptureSupported)
		{
			await Shell.Current.DisplayAlert("Error", "Camera not available", "OK");
			return;
		}
	}


	private async Task AddToGallery(bool isVideo)
	{
		// return if the app is already performing another operation
		if (IsBusy)
		{
			return;
		}

		// show an error message if the camera is not available on the device
		if (!_mediaPicker.IsCaptureSupported)
		{
			await Shell.Current.DisplayAlert("Error", "Camera not available", "OK");
			return;
		}

		try
		{
			// mark as busy while using the camera
			IsBusy = true;

			FileResult file = isVideo ? await _mediaPicker.PickVideoAsync() : await _mediaPicker.PickPhotoAsync();

			// file is null if the user cancels the operation
			if (file != null)
			{
				string localFilePath = Path.Combine(GalleryFolder, file.FileName);

				// save the file into the gallery folder
				using FileStream localFileStream = File.OpenWrite(localFilePath);
#if WINDOWS
				// on Windows file.OpenReadAsync() throws an exception
				using Stream sourceStream = File.OpenRead(file.FullPath);
#else
				using Stream sourceStream = await file.OpenReadAsync();
#endif

				await sourceStream.CopyToAsync(localFileStream);

				// add the file path to the list to display the picture on the main page
				Files.Add(new CaptureDetails()
				{
					FileName = file.FileName,
					Path = localFilePath,
				});
			}
		}
		catch (Exception ex)
		{
			await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
		}
		finally
		{
			IsBusy = false;
		}
	}
}
