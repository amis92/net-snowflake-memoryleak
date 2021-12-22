# Memory leak repro for net-snowflake-connector v2

It appears that v2.x of `snowflake-connector-net`](https://github.com/snowflakedb/snowflake-connector-net) (`Snowflake.Data`)
has a memory leak when opening a connection.

## Repro Steps

Follow the steps below to run the same program with v1.x and v2.x of the library.
You need to have the `SNOWFLAKE_CONNECTIONSTRING` environment variable set.

```sh
docker build -t sfml-sf1 .
docker build -t sfml-sf2 --build-arg SnowflakePackageVersion=2.0.3 .
docker run -it --rm -e SNOWFLAKE_CONNECTIONSTRING sfml-sf1
docker run -it --rm -e SNOWFLAKE_CONNECTIONSTRING sfml-sf2
```

## Repro output

```
Using snowflake Snowflake.Data, Version=1.2.4.0, Culture=neutral, PublicKeyToken=null
Opening connection
- PrivateMemory 54.99 MB
- WorkingSet 25.59 MB
- ManagedMemory 0.10 MB
Building command
- PrivateMemory 126.79 MB
- WorkingSet 62.96 MB
- ManagedMemory 0.89 MB
Executing command
- PrivateMemory 126.83 MB
- WorkingSet 62.96 MB
- ManagedMemory 0.90 MB
Executed command
- PrivateMemory 127.18 MB
- WorkingSet 64.00 MB
- ManagedMemory 1.16 MB
Connection disposed.
- PrivateMemory 127.24 MB
- WorkingSet 64.00 MB
- ManagedMemory 1.20 MB

Using snowflake Snowflake.Data, Version=2.0.3.0, Culture=neutral, PublicKeyToken=null
Opening connection
- PrivateMemory 54.99 MB
- WorkingSet 25.75 MB
- ManagedMemory 0.10 MB
Building command
- PrivateMemory 296.36 MB
- WorkingSet 222.34 MB
- ManagedMemory 0.96 MB
Executing command
- PrivateMemory 296.39 MB
- WorkingSet 222.34 MB
- ManagedMemory 0.97 MB
Executed command
- PrivateMemory 296.73 MB
- WorkingSet 223.34 MB
- ManagedMemory 1.20 MB
Connection disposed.
- PrivateMemory 296.86 MB
- WorkingSet 223.34 MB
- ManagedMemory 1.25 MB
```

Observe WorkingSet size: for v1 it's 64 MB, for v2 it's close to 224 MB, which is over 3x as much.
