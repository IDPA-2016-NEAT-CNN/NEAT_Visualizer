﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using NEAT_Visualizer.Business;
using NEAT_Visualizer.Interaction.Commands;
using NEAT_Visualizer.Interaction.UserInteractions;
using NEAT_Visualizer.Model;
using NEAT_Visualizer.ViewModels.Dialogs;
using PropertyChanged;

namespace NEAT_Visualizer.ViewModels
{
  [ImplementPropertyChanged]
  public class MainWindowViewModel : ViewModelBase
  {
     private const char DELIMITER = '\t';

    private readonly IVisualizerBusiness business;

    private IList<Generation> generations => business.Generations;
    private IList<Species> species => SelectedGeneration >= 0 ?  generations[SelectedGeneration].Species : new List<Species>();
    private IList<NeuralNetwork> networks => SelectedSpecies >= 0 ? species[SelectedSpecies].Networks : new List<NeuralNetwork>();
    private NeuralNetwork currentNetwork => SelectedNetwork >= 0 ? networks[SelectedNetwork] : null;

    #region ctors and initializers
    public MainWindowViewModel(IVisualizerBusiness business)
    {
      this.business = business;
      InitCommands();
    }

    private void InitCommands()
    {
      OpenFileCommand = new DelegateCommand(OnOpenFile);
      OpenFolderCommand = new DelegateCommand(OnOpenFolder);
      CloseCommand = new DelegateCommand(OnClose);
    }
    #endregion

    #region InteractionRequests
    public static InteractionRequest ShowInfoInteractionRequest { get; } = InteractionRequest.Register();

    public static InteractionRequest OpenFileDialogInteractionRequest { get; } = InteractionRequest.Register();
    #endregion

    #region Commands
    public ICommand OpenFileCommand { get; private set; }

    public ICommand OpenFolderCommand { get; private set; }

    public ICommand CloseCommand { get; private set; }
    #endregion

    #region Properties
    public int SelectedGeneration { get; set; }
    // ReSharper disable once UnusedMember.Local
    private void OnSelectedGenerationChanged()
    {
      SelectedSpecies = 0;
      SelectedNetwork = 0;
    }

    public int SelectedSpecies { get; set; }
    // ReSharper disable once UnusedMember.Local
    private void OnSelectedSpeciesChanged()
    {
      SelectedNetwork = 0;
    }

    public int SelectedNetwork { get; set; }

    public ObservableCollection<string> Generations
      =>
        new ObservableCollection<string>(
          generations.Select(g => $" {g.GenerationsPassed}{DELIMITER}{g.Species[0].FitnessHighscore}"));
    // ReSharper disable once UnusedMember.Local

    public ObservableCollection<string> Species
      => new ObservableCollection<string>(
        species.Select((s, i) => $" {i}{DELIMITER}{s.FitnessHighscore}"));

    public ObservableCollection<string> Networks
      => new ObservableCollection<string>(
        networks.Select((n, i) => $" {i}{DELIMITER}{n.Fitness/**n.FitnessModifier*/}"));
    #endregion

    #region CommandHandlers
    private void OnOpenFile()
    {
      var interaction = new UserInteraction()
      {
        Title = "Select generation file",
        Content = new OpenFileDialogViewModel()
      };

      OpenFileDialogInteractionRequest.Raise(interaction, OnOpenFileCallback);
    }

    private void OnOpenFileCallback(IUserInteraction interaction)
    {
      if (interaction.UserInteractionResult == UserInteractionOptions.Ok)
      {
        business.Generations.Clear();
        business.Generations.Add(business.NetworkLoader.LoadGeneration((interaction.Content as OpenFileDialogViewModel).SelectedFile));
        OnPropertyChanged(null);
      }

    }

    private void OnOpenFolder()
    {

    }

    private void OnClose()
    {
      Application.Current.Exit();
    }
    #endregion
  }
}