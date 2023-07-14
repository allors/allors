﻿namespace Workspace.ViewModels.Features;

using System.ComponentModel;
using Allors.Workspace;
using Allors.Workspace.Domain;

public class GreetingAdapter : IDisposable
{
    public GreetingAdapter(Person person, IPropertyChange propertyChange, string propertyName = "Greeting")
    {
        this.Person = person;
        this.ChangeNotification = new WeakReference<IPropertyChange>(propertyChange);
        this.PropertyName = propertyName;

        this.Roles = new IRole[] { this.Person.FirstName, this.Person.LastName, };

        foreach (var role in this.Roles)
        {
            role.PropertyChanged += this.Role_PropertyChanged;
        }

        this.Calculate();
    }

    public Person Person { get; }

    public IRole[] Roles { get; private set; }

    public WeakReference<IPropertyChange> ChangeNotification { get; private set; }

    public string PropertyName { get; }

    public string Value
    {
        get;
        private set;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        foreach (var role in this.Roles)
        {
            role.PropertyChanged -= this.Role_PropertyChanged;
        }
    }

    private void Calculate()
    {
        this.Value = $"Hello {this.Person.FirstName.Value}";
    }

    private void Role_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (!this.ChangeNotification.TryGetTarget(out var changeNotification))
        {
            this.Dispose();
            return;
        }

        this.Calculate();
        changeNotification.OnPropertyChanged(new PropertyChangedEventArgs(this.PropertyName));
    }

}