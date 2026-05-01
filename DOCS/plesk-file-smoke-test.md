# Plesk file smoke test

This is the first safe CI/CD step. It does not build or deploy the app. It only writes or deletes one file named `test.txt` in a test folder on Plesk.

## GitHub Secrets

Add these in GitHub repository settings:

- `PLESK_FILE_HOST`: `ftp://pleskserver.hostingdunyam.net`
- `PLESK_FILE_USERNAME`: FTP/SFTP username
- `PLESK_FILE_PASSWORD`: FTP/SFTP password
- `PLESK_TEST_REMOTE_DIR`: `/tummenu.com/webapp/ci-cd-smoke-test`

Optional:

- `PLESK_FTP_SSL_FORCE`: defaults to `true`
- `PLESK_FTP_SSL_VERIFY`: defaults to `true`

If the FTP certificate is self-signed, set `PLESK_FTP_SSL_VERIFY` to `false`.

## Test flow

1. Run `Plesk file smoke test` manually from GitHub Actions.
2. Choose `write` and set content like `first test`.
3. Confirm in Plesk File Manager that `test.txt` exists under `PLESK_TEST_REMOTE_DIR`.
4. Run it again with `write` and different content to test editing.
5. Run it with `delete` to remove only `test.txt`.

The real deploy workflow should only be added after this file-level smoke test passes.

Verified manually on April 30, 2026: FTP listing worked, `test.txt` was written, edited, read back, and deleted under `/tummenu.com/webapp/ci-cd-smoke-test`.

## Next gated test

`Plesk WebUI build test` is the next manual workflow. It builds, optionally tests, publishes `WebUI`, and uploads the publish output as a GitHub workflow artifact for inspection. It does not deploy app files.

If `deploy_marker` is checked, it uploads only one file named `ci-cd-build-marker.txt` to the WebUI remote folder. Add this secret before using that option:

- `PLESK_WEBAPP_REMOTE_DIR`: `/tummenu.com/webapp`

Keep `run_tests` enabled for CI confidence. It can be disabled only when testing publish artifact generation while existing tests are known to be failing.

## Static file deploy gate

`Plesk WebUI static file deploy` is the next deploy gate. It is manual and uploads only one approved static file:

- `WebUI/wwwroot/css/public-menu.css` -> `/tummenu.com/webapp/wwwroot/css/public-menu.css`

It does not delete files, does not write `web.config`, and does not touch `location-api`.

## Guarded WebUI deploy

`Plesk WebUI guarded deploy` is the first full WebUI deploy gate. It runs automatically on every push to `main`, and can also be started manually.

Automatic `main` deployments always run tests before uploading to Plesk.

Manual runs have two modes:

- `plan`: builds, tests, publishes, removes protected paths from the deploy package, and uploads the package as a GitHub artifact for inspection. It does not upload to Plesk.
- `deploy`: does the same package preparation, then uploads the guarded package to `/tummenu.com/webapp`.

Protected paths are removed from the package and also excluded during FTP upload:

- `web.config`
- `appsettings*.json`
- `location-api/**`
- `Logs/**`
- `logs/**`
- `wwwroot/uploads/**`

This workflow does not use delete-based mirroring. Files that exist on Plesk but are not in the package are left in place.

Deploys use a `.ci-cd-webui-manifest.sha256` file on the Plesk WebUI root to compare file content hashes. The first deploy after adding the manifest may upload the full guarded package once. Later deploys upload only files whose content hash changed.
