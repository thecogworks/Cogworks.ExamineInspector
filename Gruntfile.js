module.exports = function(grunt) {

    var path = require('path');

    // load the package JSON file
    var pkg = grunt.file.readJSON('package.json');

    // get the project paths
    var sourceDir = 'src/',
        projectDir = sourceDir + pkg.name + '/';

    // get the release paths
    var releaseDir = 'release/',
        releaseFilesDir = releaseDir + 'files/';

    // load information about the assembly
    var assembly = grunt.file.readJSON(projectDir + 'Properties/AssemblyInfo.json');

    // get the version of the package
    var version = assembly.informationalVersion ? assembly.informationalVersion : assembly.version;

    grunt.initConfig({
        pkg: pkg,
        clean: {
            files: [
                releaseFilesDir + '**/*.*'
            ]
        },
        copy: {
            bacon: {
                files: [
                    {
                        expand: true,
                        cwd: projectDir + 'obj/Release/',
                        src: [
                            pkg.name + '.dll',
                            pkg.name + '.xml'
                        ],
                        dest: releaseFilesDir + 'bin/'
                    },
                    {
                        expand: true,
                        cwd: projectDir + 'Web/UI/',
                        src: ['**'],
                        dest: releaseFilesDir
                    }
                ]
            }
        },
        zip: {
            release: {
                cwd: releaseFilesDir,
                src: [
                    releaseFilesDir + '**/*.*'
                ],
                dest: releaseDir + 'zip/' + pkg.name + '.' + version + '.zip'
            }
        },
        umbracoPackage: {
            dist: {
                src: releaseFilesDir,
                dest: releaseDir + 'umbraco/',
                options: {
                    name: pkg.name,
                    version: version,
                    url: pkg.url,
                    license: pkg.license.name,
                    licenseUrl: pkg.license.url,
                    author: pkg.author.name,
                    authorUrl: pkg.author.url,
                    readme: pkg.readme,
                    outputName: pkg.name + '.' + version + '.zip'
                }
            }
        },
        nugetpack: {
            dist: {
                src: projectDir + pkg.name + '.csproj',
                dest: releaseDir + 'nuget/',
				options: {
					properties: 'Platform=AnyCPU;Configuration=Release;',
					build: true
				}
            }
        },
		bump: {	
			options: {
				files: ['package.json','src/Cogworks.ExamineInspector/Properties/AssemblyInfo.json'],
				commit: false,
				createTag: true,
				tagName: '%VERSION%',
				tagMessage: '%VERSION%',
				push: false,
				pushTo: 'origin',
				globalReplace: true,
				regExp: false
			}
			
		},
		msbuild: {
			dev: {
				src: ['src/Cogworks.ExamineInspector/Cogworks.ExamineInspector.csproj'],
				options: {
					projectConfiguration: 'Release',
					targets: ['Clean', 'Rebuild'],
					verbosity: 'quiet'
				}
			}
		},
		tag: {
			options: {
				tagName: '<%= version %>',
				tagMsg: '<%= version %>'
			}
		},
		git: {
			commit: {
				options: {
					a: true,
					m: 'Woot!! '
				}
			},
			pushOrigin: {
			  options: {
				simple: {
				  cmd: 'push',
				  args: ['origin', 'master']
				}
			  }
			}
		}
    });

    grunt.loadNpmTasks('grunt-umbraco-package');
    grunt.loadNpmTasks('grunt-contrib-clean');
    grunt.loadNpmTasks('grunt-contrib-copy');
    grunt.loadNpmTasks('grunt-nuget');
    grunt.loadNpmTasks('grunt-zip');
	grunt.loadNpmTasks('grunt-bump');
	grunt.loadNpmTasks('grunt-msbuild');
	grunt.loadNpmTasks('grunt-tag');
	grunt.loadNpmTasks('grunt-simple-git');

    grunt.registerTask('dev', ['copy', 'zip', 'umbracoPackage', 'nugetpack']);
	
	grunt.registerTask('default', ['bump','msbuild','git:commit','tag','git:pushOrigin','dev']);

};