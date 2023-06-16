# Open Source Public Fund Experiment - Helper tools

Files and instructions to convert Open Collective API - Accounts data to CSV format

- [Open source public fund experiment - article](https://dev.to/coni2k/open-source-public-fund-experiment-lc8)
- [Open source public fund experiment - sheet](https://docs.google.com/spreadsheets/d/1JsSie6KiIV7DZttjy5CocKY-SZkxOvLVVbOJlM8SIYU/edit?usp=sharing)

## Folder and Visual Studio Code setup
- To use Open Collective API, create a personal token under `Settings - For developers - Create Personal token`:
https://opencollective.com/
- Create an `.env` file under the `rest-client` folder
- Add your token to the file with the following format:
`Token = "[Your personal token]"
- Download [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) extension
- Download [JSON to CSV](https://marketplace.visualstudio.com/items?itemName=khaeransori.json2csv) extension
- You're all set!

## Steps 
- Open `accounts.http` file, send a request and copy the response body
- Open `data-converter.html`, paste the data and click the `Convert` button. The output will be copied to the clipboard.
- Create a new file in VS Code, paste the JSON output and convert it to CSV using the extension.
