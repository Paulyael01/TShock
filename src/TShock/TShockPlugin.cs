﻿// Copyright (c) 2019 Pryaxis & TShock Contributors
// 
// This file is part of TShock.
// 
// TShock is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// TShock is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with TShock.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Orion;
using Orion.Events.Server;
using Orion.Players;
using TShock.Commands;
using TShock.Modules;

namespace TShock {
    /// <summary>
    /// Represents the TShock plugin.
    /// </summary>
    public sealed class TShockPlugin : OrionPlugin {
        private readonly Lazy<IPlayerService> _playerService;
        private readonly Lazy<ICommandService> _commandService;

        private readonly ISet<TShockModule> _modules = new HashSet<TShockModule>();

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        public override string Author => "Pryaxis";

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        public override string Name => "TShock";

        /// <summary>
        /// Initializes a new instance of the <see cref="TShockPlugin"/> class with the specified Orion kernel and
        /// services.
        /// </summary>
        /// <param name="kernel">The Orion kernel.</param>
        /// <param name="playerService">The player service.</param>
        /// <param name="commandService">The command service.</param>
        /// <exception cref="ArgumentNullException">Any of the services are <see langword="null"/>.</exception>
        public TShockPlugin(OrionKernel kernel, Lazy<IPlayerService> playerService,
                Lazy<ICommandService> commandService) : base(kernel) {
            Kernel.Bind<ICommandService>().To<TShockCommandService>();

            _playerService = playerService ?? throw new ArgumentNullException(nameof(playerService));
            _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        }

        /// <summary>
        /// Registers the given <paramref name="module"/>.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <exception cref="ArgumentNullException"><paramref name="module"/> is <see langword="null"/>.</exception>
        public void RegisterModule(TShockModule module) {
            if (module is null) {
                throw new ArgumentNullException(nameof(module));
            }

            _modules.Add(module);
        }

        /// <inheritdoc/>
        public override void Initialize() {
            Kernel.ServerInitialize.RegisterHandler(ServerInitializeHandler);
            RegisterModule(new CommandModule(Kernel, _playerService.Value, _commandService.Value));
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposeManaged) {
            Kernel.ServerInitialize.UnregisterHandler(ServerInitializeHandler);
            foreach (var module in _modules) {
                module.Dispose();
            }
        }

        private void ServerInitializeHandler(object sender, ServerInitializeEventArgs args) {
            foreach (var module in _modules) {
                module.Initialize();
            }
        }
    }
}