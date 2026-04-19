using My_money.Enums;
using My_money.Model;
using My_money.Services.IServices;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace My_money.ViewModel
{
    public class HistoryViewModel : ViewModelBase
    {
        #region Dependency Injection Services
        private readonly IRecordService _recordService;
        private readonly IUserSessionService _userSessionService;
        #endregion

        public HistoryViewModel(IRecordService recordService, IUserSessionService userSessionService)
        {
            #region Dependency Injection
            _recordService = recordService;
            _userSessionService = userSessionService;
            #endregion

            DeleteCommand = new MyICommand<object>(OnDelete);

            InitializeContexts();
            _ = LoadDataAsync();
        }

        public MyICommand<object> DeleteCommand { get; }

        #region Properties
        private ObservableCollection<ContextOption> availableContexts = new();
        public ObservableCollection<ContextOption> AvailableContexts
        {
            get => availableContexts;
            set => SetProperty(ref availableContexts, value);
        }

        private ContextOption selectedContext;
        public ContextOption SelectedContext
        {
            get => selectedContext;
            set
            {
                if (selectedContext == value || value is null)
                {
                    return;
                }

                SetProperty(ref selectedContext, value);
                OnPropertyChanged(nameof(ContextDescription));
                _ = LoadDataAsync();
            }
        }

        public string ContextDescription => SelectedContext?.FilterType switch
        {
            CategoryFilterType.Household => "Shared household records across all members.",
            CategoryFilterType.Personal => "Private records created for the signed-in user only.",
            CategoryFilterType.Child when IsChild => "Shared records created by this child account.",
            CategoryFilterType.Child => "Shared records created by the signed-in user inside the household.",
            _ => "Record timeline"
        };

        private ObservableCollection<Record> records = new();
        public ObservableCollection<Record> Records
        {
            get => records;
            set => SetProperty(ref records, value);
        }

        private Record selectedItem;
        public Record SelectedItem
        {
            get => selectedItem;
            set => SetProperty(ref selectedItem, value);
        }

        private double selectedSort;
        public double SelectedSort
        {
            get => selectedSort;
            set
            {
                SetProperty(ref selectedSort, value);
                SortRecords();
            }
        }
        #endregion

        public bool CanDeleteRecords => !IsChild;

        private int HouseholdId => _userSessionService.CurrentHouseholdMember?.HouseholdId ?? 0;
        private bool IsChild => _userSessionService.CurrentHouseholdMember?.Role == nameof(HouseholdMemberRole.Child);

        private void InitializeContexts()
        {
            AvailableContexts.Clear();
            AvailableContexts.Add(new ContextOption
            {
                Title = "Household",
                FilterType = CategoryFilterType.Household,
                UsesHouseholdFinance = true
            });

            if (!IsChild)
            {
                AvailableContexts.Add(new ContextOption
                {
                    Title = "Personal",
                    FilterType = CategoryFilterType.Personal,
                    UsesHouseholdFinance = false
                });
            }
            else
            {
                AvailableContexts.Add(new ContextOption
                {
                    Title = "My shared activity",
                    FilterType = CategoryFilterType.Child,
                    UsesHouseholdFinance = true
                });
            }

            SelectedContext = AvailableContexts.First();
            OnPropertyChanged(nameof(CanDeleteRecords));
        }

        private async Task LoadDataAsync()
        {
            var recordsFromDb = SelectedContext.FilterType switch
            {
                CategoryFilterType.Household => await _recordService.GetAllByHouseholdIdAsync(HouseholdId),
                CategoryFilterType.Personal => await _recordService.GetAllByOwnerAsync(),
                CategoryFilterType.Child => await _recordService.GetAllByHouseholdAndCreatedByAsync(HouseholdId),
                _ => throw new ArgumentOutOfRangeException()
            };

            Records = new ObservableCollection<Record>(recordsFromDb);
            SortRecords();
        }

        private async Task OnDelete(object _)
        {
            if (IsChild)
            {
                MessageBox.Show("Child accounts can add shared records, but they cannot delete them.", "Action not allowed", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                if (SelectedItem is null)
                {
                    throw new InvalidOperationException("No item selected for deletion.");
                }

                await _recordService.DeleteRecordAsync(SelectedItem);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while deleting the record: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SortRecords()
        {
            Records = SelectedSort switch
            {
                1 => new ObservableCollection<Record>(Records.OrderByDescending(item => item.Amount)),
                _ => new ObservableCollection<Record>(Records.OrderByDescending(item => item.DateTimeOccurred))
            };
        }
    }
}
