using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using Wahehouse.Gui.Helpers;
using Warehouse.App;
using Warehouse.Domain.Entities;
using Warehouse.Domain.Parameters;

namespace Wahehouse.Gui.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
		private Image _previewImage;
		private PreviewRenderer.PreviewRenderer _previewRenderer;
		private int _progressBarValue;

		public Image PreviewImage
		{
			get => _previewImage;
			set => Set(()=>PreviewImage, ref _previewImage, value);
		}

		public int ProgressBarValue
		{
			get => _progressBarValue;
			set => Set(() => ProgressBarValue, ref _progressBarValue, value);
		}


		public MainViewModel()
		{
			var generator = new LayoutGenerator();
			var layout = generator.GenerateLayout(60/4, 80, new[] { 20, 60 })
								  .FillWithArticles(80, 1000, 20)
								  .GetLayout();

			var orders = generator.GetPickingOrders(5, 100, 100);
			
			layout.PickingRoutesCalculationProgress += (sender, args) =>
			{
				ProgressBarValue = ((100 * 100 *  args.Done) / args.Todo);
			};

			_previewRenderer = new PreviewRenderer.PreviewRenderer(layout);
			PreviewImage = _previewRenderer.GetLayoutPreview();

			Task.Run(() => FindPaths(orders, layout));
		}

		private void FindPaths(List<PickingOrder> orders, WarehouseLayout layout)
		{
            var pickingSolver = new PickingSolver(layout);
            var distanceSolverResult = new List<PathFindingResult<PickingTravelStep>>();
            foreach (var pickingOrder in orders)
            {
                var result = pickingSolver.FindPath(pickingOrder);
                Dispatcher.CurrentDispatcher.Invoke(() =>
                {
                    PreviewImage = _previewRenderer.GetPickingPathPreview(result);
                });
                distanceSolverResult.Add(result);
            }

            var pickingScanSolver = new PickingScanSolver(layout);
			var scanSolverResult = new List<PathFindingResult<PickingTravelStep>>();
			foreach (var pickingOrder in orders)
			{
				var result = pickingScanSolver.FindPath(pickingOrder);
				Dispatcher.CurrentDispatcher.Invoke(() =>
				{
					PreviewImage = _previewRenderer.GetPickingPathPreview(result);
				});
				scanSolverResult.Add(result);
			}

			var distanceSolverTotalDistance = distanceSolverResult.SelectMany(x => x.PathCoordinates).Count();
			var scanSolverTotalDistance = scanSolverResult.SelectMany(x => x.PathCoordinates).Count();
			Dispatcher.CurrentDispatcher.Invoke(() =>
			{
				PreviewImage = _previewRenderer.GetPickingPathsPreview(distanceSolverResult);
				var path1 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"distance {distanceSolverTotalDistance}.png");
				var path2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"scan {scanSolverTotalDistance}.png");

				PreviewImage.Save(path1,ImageFormat.Png);
				PreviewImage = _previewRenderer.GetPickingPathsPreview(scanSolverResult);

				PreviewImage.Save(path2,ImageFormat.Png);
			});
		}

    }
}