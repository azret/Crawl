# Crawl

Crawl is a command line tool used to extract large amounts of text from the Web for language analysis.

## Command line parameters

### --url <string>

Specifies the starting URL to be crawled.

```
crawl --url https://www.w3.org/TR/html5/
```

### --depth <int>

Specifies the allowed depth level.

```
crawl --url https://www.w3.org/TR/html5/ --depth 3
```

### --cache <path>

Specifies the folder for storing the downloaded content.

```
crawl --url https://www.w3.org/TR/html5/ --cache data
```

### --verbose

If this flag is specified, the text is printed out to console during the crawl process.

```
crawl --url https://www.w3.org/TR/html5/ --verbose
```