// These tests share a single SQL Server container and clean the Users table between
// cases, so they must not run in parallel with each other.
[assembly: NonParallelizable]
