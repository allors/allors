﻿namespace Workspace.Blazor.ViewModels.Features.Person.List;

using Allors.Workspace;
using Allors.Workspace.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Allors.Workspace.Domain;
using Task = Task;
using Allors.Workspace.Meta;
using ViewModels.Services;

public partial class PageViewModel : ObservableObject
{
    private PersonViewModel selected;

    public PageViewModel(IWorkspace workspace, IMessageService messageService)
    {
        this.Workspace = workspace;
        this.MessageService = messageService;
    }

    public IWorkspace Workspace { get; set; }

    public IMessageService MessageService { get; }

    public ObservableCollection<PersonViewModel> People { get; } = new();

    public PersonViewModel Selected
    {
        get => this.selected;
        set
        {
            this.SetProperty(ref this.selected, value);
        }
    }

    [RelayCommand]
    private void ShowDialog()
    {
        var result = this.MessageService.ShowDialog("Yes or No?", "The Question");
        Console.WriteLine(result);
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        var m = this.Workspace.Services.Get<M>();

        var pull = new Pull
        {
            Extent = new Filter(m.Person),
        };

        var result = await this.Workspace.PullAsync(pull);
        var people = result.GetCollection<Person>();

        this.People.Clear();
        foreach (var person in people)
        {
            this.People.Add(new PersonViewModel(person));
        }

        this.OnPropertyChanged(nameof(People));
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        var result = await this.Workspace.PushAsync();

        if (result.HasErrors)
        {
            this.MessageService.Show(result.ErrorMessage, "Error");
            return;
        }

        this.Workspace.Reset();

        await this.LoadAsync();
    }
}