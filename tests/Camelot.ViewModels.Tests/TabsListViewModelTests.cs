using System;
using System.Collections.Generic;
using System.Linq;
using Camelot.DataAccess.Models;
using Camelot.Services.Abstractions;
using Camelot.ViewModels.Factories.Interfaces;
using Camelot.ViewModels.Implementations.MainWindow.FilePanels;
using Camelot.ViewModels.Interfaces.MainWindow.FilePanels;
using Moq;
using Moq.AutoMock;
using Xunit;

namespace Camelot.ViewModels.Tests
{
    public class TabsListViewModelTests
    {
        private const string AppRootDirectory = "Root";
        private const string Directory = "Dir";

        private readonly AutoMocker _autoMocker;

        public TabsListViewModelTests()
        {
            _autoMocker = new AutoMocker();
        }

        [Fact]
        public void TestEmptyFilesPanelState()
        {
            var tabViewModel = new Mock<ITabViewModel>().Object;
            _autoMocker
                .Setup<ITabViewModelFactory, ITabViewModel>(m => m.Create(It.IsAny<TabModel>()))
                .Returns(tabViewModel);
            _autoMocker
                .Setup<IFilesPanelStateService, PanelModel>(m => m.GetPanelState())
                .Returns(PanelModel.Empty)
                .Verifiable();

            _autoMocker
                .Setup<IDirectoryService, string>(m => m.GetAppRootDirectory())
                .Returns(AppRootDirectory)
                .Verifiable();
            _autoMocker
                .Setup<IDirectoryService, bool>(m => m.CheckIfExists(AppRootDirectory))
                .Returns(true);

            var tabsListViewModel = _autoMocker.CreateInstance<TabsListViewModel>();

            _autoMocker.Verify<IFilesPanelStateService, PanelModel>(m => m.GetPanelState(), Times.Once);
            _autoMocker.Verify<IDirectoryService, string>(m => m.GetAppRootDirectory(), Times.Once);

            Assert.Single(tabsListViewModel.Tabs);
            Assert.Equal(tabViewModel, tabsListViewModel.SelectedTab);
        }

        [Fact]
        public void TestNonEmptyFilesPanelState()
        {
            var tabsCount = new Random().Next(2, 10);

            var tabViewModel = new Mock<ITabViewModel>().Object;
            _autoMocker
                .Setup<ITabViewModelFactory, ITabViewModel>(m => m.Create(It.IsAny<TabModel>()))
                .Returns(tabViewModel);
            _autoMocker
                .Setup<IFilesPanelStateService, PanelModel>(m => m.GetPanelState())
                .Returns(new PanelModel
                {
                    Tabs = Enumerable
                        .Repeat(new TabModel {Directory = AppRootDirectory}, tabsCount)
                        .ToList()
                })
                .Verifiable();

            _autoMocker
                .Setup<IDirectoryService, bool>(m => m.CheckIfExists(AppRootDirectory))
                .Returns(true);

            var tabsListViewModel = _autoMocker.CreateInstance<TabsListViewModel>();

            _autoMocker.Verify<IFilesPanelStateService, PanelModel>(m => m.GetPanelState(), Times.Once);

            Assert.Equal(tabsCount, tabsListViewModel.Tabs.Count());
        }

        [Fact]
        public void TestCreateAndCloseTab()
        {
            var sortingViewModelMock = new Mock<IFileSystemNodesSortingViewModel>();
            var tabViewModelMock = new Mock<ITabViewModel>();
            tabViewModelMock
                .SetupGet(m => m.SortingViewModel)
                .Returns(sortingViewModelMock.Object);
            _autoMocker
                .Setup<ITabViewModelFactory, ITabViewModel>(m => m.Create(It.IsAny<TabModel>()))
                .Returns(tabViewModelMock.Object);
            _autoMocker
                .Setup<IFilesPanelStateService>(m => m.SavePanelState(It.IsAny<PanelModel>()))
                .Verifiable();
            _autoMocker
                .Setup<IFilesPanelStateService, PanelModel>(m => m.GetPanelState())
                .Returns(new PanelModel
                {
                    Tabs = new List<TabModel>
                    {
                        new TabModel {Directory = AppRootDirectory}
                    }
                });

            _autoMocker
                .Setup<IDirectoryService, bool>(m => m.CheckIfExists(AppRootDirectory))
                .Returns(true);

            var tabsListViewModel = _autoMocker.CreateInstance<TabsListViewModel>();
            Assert.Single(tabsListViewModel.Tabs);

            tabsListViewModel.CreateNewTab();
            Assert.Equal(2, tabsListViewModel.Tabs.Count());

            tabsListViewModel.CloseActiveTab();
            Assert.Single(tabsListViewModel.Tabs);
            tabsListViewModel.CloseActiveTab();
            Assert.Single(tabsListViewModel.Tabs);

            _autoMocker
                .Verify<IFilesPanelStateService>(m => m.SavePanelState(It.IsAny<PanelModel>()), Times.AtLeast(2));
        }

        [Fact]
        public void TestNoTabFound()
        {
            var sortingViewModelMock = new Mock<IFileSystemNodesSortingViewModel>();
            var tabViewModelMock = new Mock<ITabViewModel>();
            tabViewModelMock
                .SetupGet(m => m.SortingViewModel)
                .Returns(sortingViewModelMock.Object);
            _autoMocker
                .Setup<ITabViewModelFactory, ITabViewModel>(m => m.Create(It.Is<TabModel>(tm => tm.Directory == AppRootDirectory)))
                .Returns(tabViewModelMock.Object);
            _autoMocker
                .Setup<IDirectoryService, bool>(m => m.CheckIfExists(AppRootDirectory))
                .Returns(true);
            _autoMocker
                .Setup<IDirectoryService, string>(m => m.GetAppRootDirectory())
                .Returns(AppRootDirectory);
            _autoMocker
                .Setup<IFilesPanelStateService, PanelModel>(m => m.GetPanelState())
                .Returns(new PanelModel
                {
                    Tabs = new List<TabModel>
                    {
                        new TabModel {Directory = Directory},
                        new TabModel {Directory = Directory}
                    },
                    SelectedTabIndex = 1
                });

            var tabsListViewModel = _autoMocker.CreateInstance<TabsListViewModel>();
            Assert.Single(tabsListViewModel.Tabs);
        }

        [Fact]
        public void TestTabActivation()
        {
            var sortingViewModelMock = new Mock<IFileSystemNodesSortingViewModel>();
            var tabViewModelMock = new Mock<ITabViewModel>();
            tabViewModelMock
                .SetupGet(m => m.SortingViewModel)
                .Returns(sortingViewModelMock.Object);
            _autoMocker
                .Setup<ITabViewModelFactory, ITabViewModel>(m => m.Create(It.Is<TabModel>(tm => tm.Directory == AppRootDirectory)))
                .Returns(tabViewModelMock.Object);
            var secondTabViewModelMock = new Mock<ITabViewModel>();
            secondTabViewModelMock
                .SetupGet(m => m.SortingViewModel)
                .Returns(sortingViewModelMock.Object);
            _autoMocker
                .Setup<ITabViewModelFactory, ITabViewModel>(m => m.Create(It.Is<TabModel>(tm => tm.Directory == Directory)))
                .Returns(secondTabViewModelMock.Object);
            _autoMocker
                .Setup<IDirectoryService, bool>(m => m.CheckIfExists(It.Is<string>(d => d == AppRootDirectory || d == Directory)))
                .Returns(true);
            _autoMocker
                .Setup<IFilesPanelStateService, PanelModel>(m => m.GetPanelState())
                .Returns(new PanelModel
                {
                    Tabs = new List<TabModel>
                    {
                        new TabModel {Directory = AppRootDirectory},
                        new TabModel {Directory = Directory}
                    }
                });

            var tabsListViewModel = _autoMocker.CreateInstance<TabsListViewModel>();
            Assert.Equal(tabViewModelMock.Object, tabsListViewModel.SelectedTab);

            secondTabViewModelMock.Raise(m => m.ActivationRequested += null, EventArgs.Empty);
            Assert.Equal(secondTabViewModelMock.Object, tabsListViewModel.SelectedTab);
        }

        [Fact]
        public void TestNewTabAndCloseRequested()
        {
            var firstTabMock = Create();
            var currentTabMock = firstTabMock;
            _autoMocker
                .Setup<ITabViewModelFactory, ITabViewModel>(m => m.Create(It.Is<TabModel>(tm => tm.Directory == AppRootDirectory)))
                .Returns(() => currentTabMock.Object);
            _autoMocker
                .Setup<IDirectoryService, bool>(m => m.CheckIfExists(AppRootDirectory))
                .Returns(true);
            _autoMocker
                .Setup<IFilesPanelStateService, PanelModel>(m => m.GetPanelState())
                .Returns(new PanelModel
                {
                    Tabs = new List<TabModel>
                    {
                        new TabModel {Directory = AppRootDirectory}
                    }
                });

            var tabsListViewModel = _autoMocker.CreateInstance<TabsListViewModel>();
            Assert.Single(tabsListViewModel.Tabs);

            currentTabMock = Create();

            firstTabMock.Raise(m => m.NewTabRequested += null, EventArgs.Empty);
            Assert.Equal(2, tabsListViewModel.Tabs.Count());
            Assert.Equal(firstTabMock.Object, tabsListViewModel.SelectedTab);

            firstTabMock.Raise(m => m.CloseRequested += null, EventArgs.Empty);
            Assert.Single(tabsListViewModel.Tabs);
            Assert.Equal(currentTabMock.Object, tabsListViewModel.SelectedTab);
        }

        [Fact]
        public void TestCloseAllButThisRequested()
        {
            var tabs = new List<Mock<ITabViewModel>>();
            _autoMocker
                .Setup<ITabViewModelFactory, ITabViewModel>(m => m.Create(It.Is<TabModel>(tm => tm.Directory == AppRootDirectory)))
                .Returns(() =>
                {
                    var mock = Create();
                    tabs.Add(mock);

                    return mock.Object;
                });
            _autoMocker
                .Setup<IDirectoryService, bool>(m => m.CheckIfExists(AppRootDirectory))
                .Returns(true);
            _autoMocker
                .Setup<IFilesPanelStateService, PanelModel>(m => m.GetPanelState())
                .Returns(new PanelModel
                {
                    Tabs = new List<TabModel>
                    {
                        new TabModel {Directory = AppRootDirectory},
                        new TabModel {Directory = AppRootDirectory},
                        new TabModel {Directory = AppRootDirectory}
                    }
                });

            var tabsListViewModel = _autoMocker.CreateInstance<TabsListViewModel>();

            var secondTab = tabs[1];
            secondTab.Raise(m => m.ClosingAllTabsButThisRequested += null, EventArgs.Empty);

            Assert.Single(tabsListViewModel.Tabs);
            Assert.Equal(secondTab.Object, tabsListViewModel.SelectedTab);
        }

        [Fact]
        public void TestCloseAllToTheLeftRequested()
        {
            var tabs = new List<Mock<ITabViewModel>>();
            _autoMocker
                .Setup<ITabViewModelFactory, ITabViewModel>(m => m.Create(It.Is<TabModel>(tm => tm.Directory == AppRootDirectory)))
                .Returns(() =>
                {
                    var mock = Create();
                    tabs.Add(mock);

                    return mock.Object;
                });
            _autoMocker
                .Setup<IDirectoryService, bool>(m => m.CheckIfExists(AppRootDirectory))
                .Returns(true);
            _autoMocker
                .Setup<IFilesPanelStateService, PanelModel>(m => m.GetPanelState())
                .Returns(new PanelModel
                {
                    Tabs = new List<TabModel>
                    {
                        new TabModel {Directory = AppRootDirectory},
                        new TabModel {Directory = AppRootDirectory},
                        new TabModel {Directory = AppRootDirectory}
                    }
                });

            var tabsListViewModel = _autoMocker.CreateInstance<TabsListViewModel>();

            var secondTab = tabs[1];
            secondTab.Raise(m => m.ClosingTabsToTheLeftRequested += null, EventArgs.Empty);

            Assert.Equal(2, tabsListViewModel.Tabs.Count());
            Assert.Equal(secondTab.Object, tabsListViewModel.SelectedTab);
            Assert.Equal(tabs[2].Object, tabsListViewModel.Tabs.Last());
        }

        [Fact]
        public void TestCloseAllToTheRightRequested()
        {
            var tabs = new List<Mock<ITabViewModel>>();
            _autoMocker
                .Setup<ITabViewModelFactory, ITabViewModel>(m => m.Create(It.Is<TabModel>(tm => tm.Directory == AppRootDirectory)))
                .Returns(() =>
                {
                    var mock = Create();
                    tabs.Add(mock);

                    return mock.Object;
                });
            _autoMocker
                .Setup<IDirectoryService, bool>(m => m.CheckIfExists(AppRootDirectory))
                .Returns(true);
            _autoMocker
                .Setup<IFilesPanelStateService, PanelModel>(m => m.GetPanelState())
                .Returns(new PanelModel
                {
                    Tabs = new List<TabModel>
                    {
                        new TabModel {Directory = AppRootDirectory},
                        new TabModel {Directory = AppRootDirectory},
                        new TabModel {Directory = AppRootDirectory}
                    }
                });

            var tabsListViewModel = _autoMocker.CreateInstance<TabsListViewModel>();

            var secondTab = tabs[1];
            secondTab.Raise(m => m.ClosingTabsToTheRightRequested += null, EventArgs.Empty);

            Assert.Equal(2, tabsListViewModel.Tabs.Count());
            Assert.Equal(tabs[0].Object, tabsListViewModel.SelectedTab);
            Assert.Equal(secondTab.Object, tabsListViewModel.Tabs.Last());
        }

        private static Mock<ITabViewModel> Create()
        {
            var sortingViewModelMock = new Mock<IFileSystemNodesSortingViewModel>();
            var tabViewModelMock = new Mock<ITabViewModel>();
            tabViewModelMock
                .SetupGet(m => m.SortingViewModel)
                .Returns(sortingViewModelMock.Object);
            tabViewModelMock
                .SetupGet(m => m.CurrentDirectory)
                .Returns(AppRootDirectory);

            return tabViewModelMock;
        }
    }
}