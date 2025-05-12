# Peer Infrastructure
**UNDER DEVELOPMENT!!!**

**Peer Infrastructure** is a modular, native .NET implementation of peer-to-peer networking and distributed content systems. It provides developers with a robust foundation for building decentralized applications, using open protocols reimagined for the .NET ecosystem.

This infrastructure is organized into three core components:

- **PeerStack** – A shared library of primitives, abstractions, and utilities used by both PeerTalk and Armillaria.
- **PeerTalk** – A pure .NET implementation of the [libp2p](https://libp2p.io) protocol stack for modular peer-to-peer networking.
- **Armillaria** – A content-addressed, distributed storage layer inspired by [IPFS](https://ipfs.io), built on top of PeerTalk.


---

## 🎯 Project Goal

**Peer Infrastructure** aims to make peer-to-peer and decentralized application development seamless and idiomatic in C# and .NET. By combining modular networking and storage layers with reusable components, the project empowers developers to build trustless, distributed systems that are secure, scalable, and interoperable.

---

## 🧩 Components Overview

### ⚙️ PeerStack

**PeerStack** is the shared foundation that powers both PeerTalk and Armillaria. It contains:

- Cryptographic utilities  
- Protocol message definitions  
- Multiaddress and multibase handling  
- Common abstractions and serialization

It promotes consistency and code reuse across the Peer Infrastructure projects.

### 🔗 PeerTalk

PeerTalk brings the libp2p protocol suite to .NET, offering:

- Peer identity and authentication  
- Transport abstraction (TCP, WebSockets, and more)  
- Secure multiplexed streams
- PubSub and peer discovery protocols *(in progress)*
- NAT traversal and relay mechanisms *(planned)*  


It serves as the networking backbone for distributed apps within the Peer Infrastructure ecosystem.

### 🌱 Armillaria

Named after a genus of fungi known for forming large underground networks, **Armillaria** is a decentralized storage system modeled after IPFS. It features:

- Content addressing via cryptographic multihashes  
- Directed Acyclic Graph (DAG)-based object structure  
- Block exchange and pinning  
- Local and distributed content resolution

Armillaria leverages PeerTalk for transport and peer communication, enabling decentralized data availability.

---

## 🌐 Use Cases

- Decentralized applications (dApps)  
- Distributed file sharing  
- Secure and resilient messaging systems  
- Offline-first apps with peer sync  
- Collaborative tools without central servers

---

## 🚀 Getting Started

Each module is independently usable and will be published as a NuGet package. Integration guides and examples are coming soon. For now, clone the repository and explore the code structure to get familiar with the architecture.

---

## 🤝 Contributions Welcome

We invite developers, protocol enthusiasts, and open-source contributors to join us. Whether you're improving protocol support, enhancing performance, or building on top of the stack — we’d love your input.

---

## 📄 License

Peer Infrastructure is released under the MIT License.

---

## 🙏 Acknowledgements

This work builds upon the [Richard Schneider](https://github.com/richardschneider) collection of IPFS-related projects. We have retained the name **PeerTalk** for his libp2p implementation in recognition of his foundational contributions—without which this project would not have been possible. The project also draws inspiration from the groundbreaking work behind [libp2p](https://libp2p.io), [IPFS](https://ipfs.io), and the broader Protocol Labs ecosystem. Special thanks to the open-source pioneers who laid the groundwork for decentralized networking and storage.

---

**Build resilient systems. Break free from central servers. Welcome to Peer Infrastructure.**
