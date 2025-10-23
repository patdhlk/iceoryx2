// Copyright (c) 2025 Contributors to the Eclipse Foundation
//
// See the NOTICE file(s) distributed with this work for additional
// information regarding copyright ownership.
//
// This program and the accompanying materials are made available under the
// terms of the Apache Software License 2.0 which is available at
// https://www.apache.org/licenses/LICENSE-2.0, or the MIT license
// which is available at https://opensource.org/licenses/MIT.
//
// SPDX-License-Identifier: Apache-2.0 OR MIT

using System;

namespace Iceoryx2;

/// <summary>
/// Represents a node in the iceoryx2 system.
/// A node is the central entry point and represents a process in the iceoryx2 ecosystem.
/// </summary>
public sealed class Node : IDisposable
{
    internal SafeNodeHandle _handle;
    private bool _disposed;

    internal Node(SafeNodeHandle handle)
    {
        _handle = handle ?? throw new ArgumentNullException(nameof(handle));
    }

    /// <summary>
    /// Gets the name of the node.
    /// </summary>
    public string Name
    {
        get
        {
            ThrowIfDisposed();
            // TODO: Implement proper node name retrieval
            return "node"; // Placeholder
        }
    }

    /// <summary>
    /// Gets the unique ID of the node.
    /// </summary>
    public Guid Id
    {
        get
        {
            ThrowIfDisposed();
            // TODO: Implement proper node ID retrieval
            return Guid.NewGuid(); // Placeholder
        }
    }

    /// <summary>
    /// Creates a builder for creating or opening a service.
    /// </summary>
    public ServiceBuilder ServiceBuilder()
    {
        ThrowIfDisposed();
        return new ServiceBuilder(this);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _handle?.Dispose();
            _disposed = true;
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(Node));
    }
}

/// <summary>
/// Builder for creating a Node.
/// </summary>
public sealed class NodeBuilder
{
    private string? _name;

    private NodeBuilder()
    {
    }

    /// <summary>
    /// Creates a new NodeBuilder.
    /// </summary>
    public static NodeBuilder New() => new();

    /// <summary>
    /// Sets the name of the node.
    /// </summary>
    public NodeBuilder Name(string name)
    {
        _name = name ?? throw new ArgumentNullException(nameof(name));
        return this;
    }

    /// <summary>
    /// Creates the node.
    /// </summary>
    public Result<Node, Iox2Error> Create()
    {
        try
        {
            // Create node builder with proper struct
            var builderStruct = new Native.Iox2NativeMethods.iox2_node_builder_t();
            var builderHandle = Native.Iox2NativeMethods.iox2_node_builder_new(ref builderStruct);
            
            if (builderHandle == IntPtr.Zero)
                return Result<Node, Iox2Error>.Err(Iox2Error.NodeCreationFailed);

            // Set node name if provided
            if (!string.IsNullOrEmpty(_name))
            {
                var nodeNameStruct = new Native.Iox2NativeMethods.iox2_node_name_t();
                var result = Native.Iox2NativeMethods.iox2_node_name_new(
                    ref nodeNameStruct,
                    _name,
                    _name.Length,
                    out var nodeNameHandle);
                
                if (result == Native.Iox2NativeMethods.IOX2_OK)
                {
                    Native.Iox2NativeMethods.iox2_node_builder_set_name(builderHandle, nodeNameHandle);
                    Native.Iox2NativeMethods.iox2_node_name_drop(nodeNameHandle);
                }
            }

            // Create the node - pass IntPtr.Zero to let C FFI allocate the struct
            var createResult = Native.Iox2NativeMethods.iox2_node_builder_create(
                builderHandle,
                IntPtr.Zero,  // NULL - let C allocate the struct on heap
                Native.Iox2NativeMethods.iox2_service_type_e.IPC,
                out var nodeHandle);
            
            if (createResult != Native.Iox2NativeMethods.IOX2_OK || nodeHandle == IntPtr.Zero)
                return Result<Node, Iox2Error>.Err(Iox2Error.NodeCreationFailed);

            var handle = new SafeNodeHandle(nodeHandle);
            var node = new Node(handle);
            
            return Result<Node, Iox2Error>.Ok(node);
        }
        catch (Exception)
        {
            return Result<Node, Iox2Error>.Err(Iox2Error.NodeCreationFailed);
        }
    }
}
